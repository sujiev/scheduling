using LinqToDB;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Scheduling.Controllers;
using Scheduling.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scheduling
{
    public class ScheduleReceiver : BackgroundService
    {
        private readonly ApplicationDBContext _context;
        private IModel _channel;
        private IConnection _connection;
        protected readonly IServiceProvider _serviceProvider;
       
        public ScheduleReceiver(IServiceProvider serviceProvider)
        {           
            _serviceProvider = serviceProvider;
            InitializeRabbitMqListener();
        }
       

        private void InitializeRabbitMqListener()
        {            
            //connection factory
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@localhost:5672")
            };

            _connection = factory.CreateConnection(); //default conneciton
            _channel = _connection.CreateModel();

            //declare a queue
            _channel.QueueDeclare("schedule-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());

                dynamic data = JObject.Parse(content);

                FlightSchedule model = new FlightSchedule(data.Message.ToString());

                HandleMessage(model);

                _channel.BasicAck(ea.DeliveryTag, false);
            };             

            _channel.BasicConsume("schedule-queue", false, consumer);

            return Task.CompletedTask;
        }

        private async void HandleMessage(FlightSchedule model)
        {
            // (<SignalrServer>)_serviceProvider.GetService(typeof(IHubContext<SignalrServer>));

            using(IServiceScope scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
                model.ScheduleID = context.FlightSchedule.ToList().Count() + 1;
               
                await context.FlightSchedule.AddAsync(model);
                await context.SaveChangesAsync();
            }

            var signalrHub = (IHubContext<SignalrServer>)_serviceProvider.GetService(typeof(IHubContext<SignalrServer>)); 

            // Send message to all users in SignalR
           await signalrHub.Clients.All.SendAsync("LoadScheduleData");          
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}

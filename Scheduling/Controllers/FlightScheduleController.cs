using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;

namespace Scheduling.Controllers
{
    public class FlightScheduleController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly IHubContext<SignalrServer> _signalrHub;
        private static IWebHostEnvironment _webHostEnviornment;

        public FlightScheduleController(ApplicationDBContext context, IHubContext<SignalrServer> signalrHub, IWebHostEnvironment webHostEnviornment)
        {
            _context = context;
            _signalrHub = signalrHub;
            _webHostEnviornment = webHostEnviornment;
        }

        //Get FlightSchedule
        public async Task<IActionResult> Index()
        {
            return View(await _context.FlightSchedule.ToListAsync());
        }
 
        [HttpGet]
        public IActionResult GetFlightSchedule()
        {
            var res = _context.FlightSchedule.ToList();
            return Ok(res);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var data = await _context.FlightSchedule.FirstOrDefaultAsync(m => m.ScheduleID == id);
            if(data == null)
            {
                return NotFound();
            }

            return View(data);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var data = await _context.FlightSchedule.FirstOrDefaultAsync(m => m.ScheduleID == id);
            if (data == null)
            {
                return NotFound();
            }

            return View(data);
        }

        // GET: FlightSchedule/Create
        public IActionResult Create()
        {
            return View();
        }


        public IActionResult Print()
        {

            StringBuilder sb = new StringBuilder();

            sb.Append(@"
                        <html>
                        <head></head>
                        <body>
                        <div class='header'><h1>Flight Schedule</h1></div>
                                <table align='center'>
                                    <tr>
                                        <th>Flights</th>
                                        <th>Depart</th>
                                        <th>From</th>
                                        <th>To</th>
                                    </tr>");

            foreach( FlightSchedule fs in _context.FlightSchedule.ToList())
            {
                sb.AppendFormat(@"<tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                  </tr>", fs.FlightNumber, fs.DepartureTime, fs.OriginStation, fs.DestinationStation);
            }

            sb.Append(@"
                                </table>
                            </body>
                        </html>");

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "Schedule Report"                
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = sb.ToString(),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "assets", "pdfstyles.css") },
                HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Report Footer" }
            };
            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            DinkToPdf.SynchronizedConverter _converter = new SynchronizedConverter( new PdfTools());
            var file = _converter.Convert(pdf);

            return File(file, "application/pdf", "FlightSchedule.pdf");
        }

        public IActionResult Upload()
        {
            return View();
        }
        

        // POST: FlightSchedule/Create
        // To protect from overposting attacks, enable the specific properties you wan tto bind to;
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ScheduleID,FlightNumber,PeriodOfOperationFrom,PeriodOfOperationTo,FlyOnMondays,FlyOnTuesdays,FlyOnWednesdays,FlyOnThursdays,FlyOnFridays,FlyOnSaturdays,FlyOnSundays,DepartureTime,OriginStation,DestinationStation,Aircraft")] FlightSchedule schedule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(schedule);
                await _context.SaveChangesAsync();
                await _signalrHub.Clients.All.SendAsync("LoadScheduleData");                
                return RedirectToAction(nameof(Index));
            }

            return View(schedule);
        }

        private bool FlightScheduleExists(int id)
        {
            return _context.FlightSchedule.Any(e => e.ScheduleID == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int ScheduleID, [Bind("ScheduleID,FlightNumber,PeriodOfOperationFrom,PeriodOfOperationTo,FlyOnMondays,FlyOnTuesdays,FlyOnWednesdays,FlyOnThursdays,FlyOnFridays,FlyOnSaturdays,FlyOnSundays,DepartureTime,OriginStation,DestinationStation,Aircraft")] FlightSchedule schedule)
        {
            if (ScheduleID != schedule.ScheduleID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(schedule);
                    await _context.SaveChangesAsync();
                    await _signalrHub.Clients.All.SendAsync("LoadScheduleData");
                }
                catch (DbUpdateConcurrencyException)
                {

                }
                return RedirectToAction(nameof(Index));
            }

            return View(schedule);
        }


        //Get : FlightSchedule/Delete/5

        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var products = await _context.FlightSchedule.FirstOrDefaultAsync(mbox => mbox.ScheduleID == id);

            if(products == null)
            {
                return NotFound();
            }

            return View(products);
        }

        //POST: FlightSchedule/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        private async Task<IActionResult> DeleteConfirmed(int ScheduleId)
        {
            var products = await _context.FlightSchedule.FindAsync(ScheduleId);
            _context.FlightSchedule.Remove(products);
            await _context.SaveChangesAsync();
            await _signalrHub.Clients.All.SendAsync("LoadScheduleData");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]         
        public async Task<string> Upload([FromForm] UploadFile fileObj)
        {
            if(fileObj.files.Length > 0)
            {
                try
                {
                    if (!Directory.Exists(_webHostEnviornment.WebRootPath + "\\schedules\\"))
                    {
                        Directory.CreateDirectory(_webHostEnviornment.WebRootPath + "\\schedules\\");
                    }
                   
                    string filePrefix = DateTime.Now.ToString("yyyyMMddHHmmss");

                    using (FileStream filestream = System.IO.File.Create(_webHostEnviornment.WebRootPath + "\\schedules\\" + filePrefix + fileObj.files.FileName))
                    {
                        fileObj.files.CopyTo(filestream);
                        filestream.Flush();                    
                    }

                    StringBuilder sb = new StringBuilder();
                    using (StreamReader sr = new StreamReader(_webHostEnviornment.WebRootPath + "\\schedules\\" + filePrefix + fileObj.files.FileName))
                    {
                        while (sr.Peek() >= 0)
                        {
                            string schedule = sr.ReadLine();
                            // only process 40 characters string like thi: AC030101NOV20  XXX  12345670015YULYVR011
                            if (schedule.Trim().Length == 40)
                            {
                                FlightSchedule model = new FlightSchedule(schedule);
                                model.ScheduleID = _context.FlightSchedule.ToList().Count() + 1;
                                if (ModelState.IsValid)
                                {
                                    _context.Add(model);
                                    await _context.SaveChangesAsync();
                                    await _signalrHub.Clients.All.SendAsync("LoadScheduleData");
                                }
                            }
                        }
                    }
                    string fileContent = sb.ToString();
                    await _signalrHub.Clients.All.SendAsync("LoadScheduleData");
                    return "\\schedules\\" + fileObj.files.FileName;
                }
                catch (Exception exp)
                {
                    return exp.ToString();
                }
            }
            else
            {
                return "Upload Failed";
            }
        }
    }
}

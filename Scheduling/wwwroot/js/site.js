$(() => {

    //LoadScheduleData();
    var connection = new signalR.HubConnectionBuilder().withUrl("/signalrServer").build();
    connection.start();
    connection.on("LoadScheduleData", function () {
        LoadScheduleData();
    })
    LoadScheduleData();

    //<td><a href='../FlightSchedule/Edit?id=${v.ScheduleID}'>Edit</a></td></tr>

    function LoadScheduleData() {
        var tr = '';
        $.ajax({

            url: '/FlightSchedule/GetFlightSchedule',
            method: 'GET',
            success: (result) => {
                $.each(result, (k, v) => {
                    tr += `<tr><td>${v.FlightNumber}</td><td>${v.DepartureTime}</td><td>${v.OriginStation}</td><td>${v.DestinationStation}</td>`
                })
                $("#tableBody").html(tr);
               
            },
            error:(error) => {
                console.log(error)                
            }
        });

    }
})
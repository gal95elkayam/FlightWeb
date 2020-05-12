function initMap() {
    var uluru = { lat: -25.363, lng: 131.044 };
    var map = new google.maps.Map(document.getElementById('map'), {
        zoom: 4,
        center: uluru
    });
}
if (typeof $ === 'undefined') {
    alert('Bootstrap\'s JavaScript requires jQuery. jQuery must be included before Bootstrap\'s JavaScript.');
}

$(document).ready(function () {
    setInterval(function () {
        $.ajax({
            url: "https://localhost:44355/api/flights",
            success:
                function (data) {
                    //debugger;
                    //let a = data[0];
                    //$('#flightInfo').html(a.company_name); //insert text of test.php into your div
                },
        });
    }, 3000);
});

function postNewFlight(flight) {
      

}

$('#flightInfo').on('dragover', (e) => {
    e.preventDefault();
    body.classList.add('dragging');
});

$('#flightInfo').on(
    'dragover',
    function (e) {
        e.preventDefault();
        e.stopPropagation();
    }
)
$('#flightInfo').on(
    'dragenter',
    function (e) {
        e.preventDefault();
        e.stopPropagation();
    }
)
$('#div').on(
    'drop',
    function (e) {
        if (e.originalEvent.dataTransfer && e.originalEvent.dataTransfer.files.length) {
            e.preventDefault();
            e.stopPropagation();
            alert(e.originalEvent.dataTransfer.files);
        }
    }
);

function AddRow(flight) {
    var newRow = $("<tr>");
    var cols = "";

    cols += '<td>' + flight.flight_id + '</td>';
    cols += '<td>' + flight.company_name + '</td>';
    cols += '<td>' + flight.passengers + '</td>';

    cols += '<td><input type="button" class="ibtnDel btn btn-md btn-danger "  value="Delete"></td>';
    newRow.append(cols);
    $("table tbody").append(newRow);
    counter++;
};

function flightsDropHandler(ev) {
    // Prevent default behavior (Prevent file from being opened)
    ev.preventDefault();
    debugger;
    // Use DataTransfer interface to access the file(s)
    for (const file of ev.dataTransfer.files) {
        const reader = new FileReader();
        reader.onload = function (evt) {
            postNewFlight(JSON.parse(evt.target.result));
        }
        reader.onerror = function (evt) {
            console.log("error reading file");
        }
        reader.readAsText(file);
        //console.log('... file[' + i + '].name = ' + file.name);
    }
}

function flightsDragOverHandler(ev) {
    // Prevent default behavior (Prevent file from being opened)
    ev.preventDefault();
}
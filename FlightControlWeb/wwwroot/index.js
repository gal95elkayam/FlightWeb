flightsId = [];

if (typeof $ === 'undefined') {
    alert('jQuery must be included.');
}

$(document).ready(function () {
    $(".drop_box").hide();
    UpdateFlightsTables();
    setInterval(UpdateFlightsTables, 3000);

    $("#internalFlightsTable").on("click", ".ibtnDel", DeleteFlightClick);

    $(".flights").on("click", "td[informative]", FlightClick);

    $(".flights").on('drag dragstart dragend dragover dragenter dragleave drop', function (e) {
        e.preventDefault();
        e.stopPropagation();
    })

    $(".flights").on('dragover dragenter', function () {
        $(".internal").hide();
        $(".external").hide();
        $(".drop_box").show();
    })
    $(".flights").on('dragleave dragend drop', function () {
        $(".drop_box").hide();
        $(".internal").show();
        $(".external").show();
    })
    $(".flights").on('drop', flightsDropHandler);

});

// initialize the map in the map section.
function initMap() {
    const uluru = { lat: -25.363, lng: 131.044 };
    const map = new google.maps.Map(document.getElementById('map'), {
        zoom: 4,
        center: uluru
    });
}

// show the error of a http request.
function requestErrorMessage(xhr) {
    alert(xhr.responseJSON.status + " - " + xhr.responseJSON.title);
}


// get flights from database and update the flights tables.
function UpdateFlightsTables() {
    $.ajax({
        url: "/api/Flights?relative_to=2020-12-26T23:56:21Z",
        success:
            function (flights) {
                let newFlightsId = [];

                // insert new flights to tables
                for (const flight of flights) {
                    newFlightsId.push(flight.flight_id);
                    const a = $("#" + flight.flight_id);
                    if (!$("#" + flight.flight_id).length) {
                        const flightSource = flight.isExternal ? "external" : "internal";
                        $("#" + flightSource + "FlightsTable tbody").append(FlightToTableRowHTML(flight));
                    }
                }

                // remove deleted flights from tables
                for (const flightId of flightsId) {
                    if (!newFlightsId.includes(flightId)) {
                        // if the row is clicked
                        if (!$("#" + flightId + " [bold]").length) {
                            EmptyFlightInfo();
                        }

                        // remove the flight from table
                        $("#" + flightId).remove();
                    }
                }

                flightsId = newFlightsId;
            }
    });
}

// add flight according to the given flight plan text.
function AddNewFlightPlan(flightPlanText) {
    if (!IsJson(flightPlanText)) {
        alert("the text is not in a json format");
    }

    $.ajax({
        type: "POST",
        url: "/api/FlightPlan",
        contentType: "application/json",
        data: flightPlanText,
        success: function (data) {
            alert("file uploaded successfuly");
            UpdateFlightsTables();
        },
        error: requestErrorMessage
    });

}

// get a flight and return its html table row.
function FlightToTableRowHTML(flight) {
    var newRow = $("<tr id=" + flight.flight_id + ">");
    var cols = "";

    cols += '<td informative>' + flight.flight_id + '</td>';
    cols += '<td informative>' + flight.company_name + '</td>';
    cols += '<td informative>' + flight.passengers + '</td>';

    if (!flight.isExternal) {
        cols += '<td><input type="button" class="ibtnDel btn btn-md btn-danger "  value="X"></td>';
    }

    newRow.append(cols);

    return newRow;
};

// update the flight information in the flight info section.
function UpdateFlightInfo(flightId) {
    $.ajax({
        type: "GET",
        url: "/api/FlightPlan/" + flightId,
        contentType: "application/json",
        success: function (flightPlan) {
            $("#flight_info_id").html(flightPlan.id);
            $("#flight_info_lon").html(flightPlan.initial_location.longitude);
            $("#flight_info_lat").html(flightPlan.initial_location.latitude);
            $("#flight_info_pas").html(flightPlan.passengers);
            $("#flight_info_com").html(flightPlan.company_name);
            $("#flight_info_time").html(flightPlan.initial_location.date_time);
            $("#flight_info_ext").html(flightPlan.is_external.toString());
        },
        error: requestErrorMessage
    });
}

function EmptyFlightInfo() {
    $("#flight_info_id").empty();
    $("#flight_info_lon").empty();
    $("#flight_info_lat").empty();
    $("#flight_info_pas").empty();
    $("#flight_info_com").empty();
    $("#flight_info_time").empty();
    $("#flight_info_ext").empty();
}

// handle click on the delete button - delete the chosen flight.
function DeleteFlightClick() {
    const rowToRemove = this.parentElement.parentElement;
    const flightId = rowToRemove.firstChild.textContent;

    // deleting bolded row?
    if ($("#" + flightId + "[bold]").length) {
        EmptyFlightInfo();
    }

    $.ajax({
        type: "DELETE",
        url: "/api/Flights/" + flightId,
        success:
            function () {
                rowToRemove.remove();
                alert("flight " + flightId + " was deleted successfuly");
            },
        error: requestErrorMessage
    });
}

// handle click on a flight in the map or in the table.
function FlightClick() {
    const row = this.parentElement;
    const flightId = row.firstChild.textContent;

    for (const boldedRow of $("tr[bold]")) {
        boldedRow.removeAttribute("bold");
    }

    row.setAttribute("bold", "");
    UpdateFlightInfo(flightId);
}


// handle dropped files.
function flightsDropHandler(ev) {
    ev = ev.originalEvent;
    // Prevent default behavior (Prevent file from being opened)
    ev.preventDefault();
    for (const file of ev.dataTransfer.files) {
        const reader = new FileReader();
        reader.onload = function (evt) {
            AddNewFlightPlan(evt.target.result);
        }
        reader.onerror = function (evt) {
            console.log("error reading file");
        }
        reader.readAsText(file);
    }
}

// prevent file from being opened
function flightsDragOverHandler(ev) {
    // Prevent default behavior (Prevent file from being opened)
    ev.preventDefault();
    console.log("a");
}

// prevent file from being opened
function flightsDragEndHandler(ev) {
    // Prevent default behavior (Prevent file from being opened)
    ev.preventDefault();
    console.log("b");
}

// return true if the text is in json format, false otherwise.
function IsJson(text) {
    try {
        JSON.parse(text);
    } catch (e) {
        return false;
    }

    return true;
}
HOST_URL = window.location.origin;

let todayDate = moment().startOf('day');
let YM = todayDate.format('YYYY-MM');
let YESTERDAY = todayDate.clone().subtract(1, 'day').format('YYYY-MM-DD');
let TODAY = todayDate.format('YYYY-MM-DD');
let TOMORROW = todayDate.clone().add(1, 'day').format('YYYY-MM-DD');
let calendar;

$.ajax({
    url: HOST_URL + "/api/teacher/registry/config",
    method: 'GET',
    success: result => {
        let config = result.filter(r => r.GradeLevelId === userRegGroup)[0].Configuration;
        let endTime = config.StartTime.Hours;
        config.Slots.forEach(s => {
            s._start = endTime;
            endTime += s.Duration
            s._end = endTime;
        })
        
        let calendarEl = document.getElementById('kt_calendar');
        calendar = new FullCalendar.Calendar(calendarEl, {
            //plugins: ['bootstrap', 'interaction', 'dayGrid', 'timeGrid', 'list'],
            themeSystem: 'bootstrap',

            // TODO Localize today button

            height: "auto",
            contentHeight: 780,
            aspectRatio: 3,  // see: https://fullcalendar.io/docs/aspectRatio

            allDaySlot: false,
            slotMinTime: config.StartTime.Hours + ':00:00',
            slotMaxTime: endTime + ':00:00', // Or 20:00:00 for kinder
            slotLabelInterval: '01:00:00',

            locale: 'en', // TODO localize FullCalendar
            titleFormat: {
                year: 'numeric',
                month: 'long',
                day: 'numeric',
                weekday: 'long'
            },
            dayHeaderFormat: {
                weekday: 'long',
            },
            slotLabelFormat: {
                hour: '2-digit',
                minute: '2-digit',
                omitZeroMinute: false,
                meridiem: 'short'
            },

            nowIndicator: false,
            now: TODAY + 'T09:25:00', // just for demo
            firstDay: 1,
            weekends: false,
            timeZone: 'UTC',

            views: {
                timeGridWeek: {buttonText: 'week'}
            },

            initialView: 'timeGridWeek',
            initialDate: TODAY,

            editable: false,
            navLinks: true,
            eventSources: [
                {
                    url: HOST_URL + '/api/teacher/registry/fullcalendar'
                }
            ],

            eventContent: function (event, element, view) {
                // console.log(event, element);
                let rootElement = $('<div></div>');
                if (event.event._def.extendedProps.extra.subjectName !== null) {
                    rootElement.append($(`<a>Subject: ${event.event._def.extendedProps.extra.subjectName}</a>`));
                    if (event.event._def.extendedProps.extra.isPco)
                        rootElement.append($(`<a> (PCO)</a>`));
                }
                if (event.event._def.extendedProps.extra.className !== null)
                    rootElement.append($(`<br><a>Class: ${event.event._def.extendedProps.extra.className}</a>`));
                if (event.event._def.title !== null)
                    rootElement.append($(`<br><a>${event.event._def.title}</a>`));
                return {
                    domNodes: [
                        rootElement[0]
                    ]
                }
            },

            slotLabelContent: function(event) {
                let h = event.date.getUTCHours();
                let slot = config.Slots.filter(s => s._start === h)[0];
                if(slot === undefined)
                    event.text = "";
                else if(slot.CustomLabel !== null)
                    event.text = slot.CustomLabel;
            },

            eventClick: function (e) {
                if(verifyLocked() === true)
                    return;
                let h = e.el.fcSeg.start.getUTCHours();
                let slot = config.Slots.filter(s => s._start === h)[0];
                updateForm(slot, e.el.fcSeg.start, e.event._def);
                $('#editCellModal').modal('show');
            },

            dateClick: function (d) {
                if(verifyLocked() === true)
                    return;
                let h = d.date.getUTCHours();
                let slot = config.Slots.filter(s => s._start === h)[0];
                updateForm(slot, d.date);
                $('#editCellModal').modal('show');
            },

            eventSourceSuccess: function (rawEvents, xhr) {
                let todayEvents = rawEvents.filter(e => e.display !== "background");
                let weekIsLocked = todayEvents.every(e => e.extra.isLocked);
                let hasEvents = todayEvents.length > 0;
                if(weekIsLocked) {
                    $('#btn-lock-week').hide().prop('disabled', !hasEvents);
                    $('#btn-unlock-week').show().prop('disabled', !hasEvents);
                } else {
                    $('#btn-lock-week').show().prop('disabled', !hasEvents);
                    $('#btn-unlock-week').hide().prop('disabled', !hasEvents);
                }
                $('#btn-print-week').prop('disabled', !hasEvents);
                
                //let totalHours = (new Date(todayEvents[0].end) - new Date(todayEvents[0].start)) / 3600000;
                let totalHours = 0;
                todayEvents.forEach(e => {
                    totalHours += (new Date(e.end) - new Date(e.start)) / 3600000;
                })
                $('#hrs-this-week').text(totalHours);
            }
        });
        calendar.render();
    }
});

function verifyLocked() {
    let events = calendar.getEvents().filter(e => e._def.ui.display !== "background");
    let isWeekLocked = events.length > 0 && events.every(e => e._def.extendedProps.extra.isLocked);
    if(isWeekLocked)
        showNotification({message: 'This week is locked'}, 'danger');
    return isWeekLocked;
}
function showNotification(content, type) {
    $.notify(content, {
        type: type ?? "success",
        allow_dismiss: true,
        newest_on_top: true,
        mouse_over: false,
        showProgressbar: false,
        spacing: 10,
        timer: 2000,
        placement: {
            from: "top",
            align: "right"
        },
        offset: {
            x: 30,
            y: 30
        },
        delay: 1000,
        z_index: 10000,
        animate: {
            enter: 'animate__animated animate__fadeIn',
            exit: 'animate__animated animate__fadeOut'
        }
    });
}
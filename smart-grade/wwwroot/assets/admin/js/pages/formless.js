function Formless(form, options) {
    form.on('submit', function (e) {
        e.preventDefault();
        form = $(e.target);
        if (typeof options.prepare === 'function')
            options.prepare(form);
        $.ajax({
            url: form.attr('action'),
            method: form.attr('method'),
            data: form.serialize(),
            success: result => options.complete({success: true, result: result}, form),
            error: result => options.complete({success: false, result: result}, form),
        });
    });
}
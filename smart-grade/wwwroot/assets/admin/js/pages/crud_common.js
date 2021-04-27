HOST_URL = window.location.origin;

let datatable = undefined;

function InitializeFormValidation() {
    return FormValidation.formValidation(
        document.getElementById('kt_form_1'),
        {
            fields: formValidationFields,

            plugins: {
                trigger: new FormValidation.plugins.Trigger(),
                // Bootstrap Framework Integration
                bootstrap: new FormValidation.plugins.Bootstrap(),
                // Validate fields when clicking the Submit button
                submitButton: new FormValidation.plugins.SubmitButton()
            }
        }
    );
}

function InitDataTable() {
    datatable = $('#kt_datatable').KTDatatable({
        // datasource definition
        data: {
            type: 'remote',
            source: {
                read: {
                    url: HOST_URL + API_PATH + 'list',
                    method: 'GET',
                    params: {
                        summarizer: typeof summarizer !== "undefined" ? summarizer : ""
                    },
                    map: function(raw) {
                        if(typeof onSumResult !== "undefined")
                            onSumResult(raw.meta.sum)
                        return raw.data;
                    }
                },
            },
            pageSize: 10,
            saveState: false,
            serverPaging: true,
            serverFiltering: true,
            serverSorting: true,
        },

        layout: {
            scroll: false
        },

        // column sorting
        sortable: true,

        pagination: true,

        search: {
            input: $('#kt_datatable_search_query'),
            key: 'generalSearch'
        },

        // columns definition
        columns: datatableDataColumns

    });

    $('#kt_datatable_search_status, #kt_datatable_search_type').selectpicker();

    datatable.on('datatable-on-ajax-done', function(e, args) {
        // console.log(e, args)
    });

    return datatable;
}

jQuery(document).ready(function () {
    let validation = undefined;
    if(typeof formValidationFields !== "undefined")
        validation = InitializeFormValidation();
    const datatable = InitDataTable();
    const form = $('#kt_form_1');

    if(validation !== undefined) {
        $('#btn_save_create_item').on('click', e => {
            e.preventDefault();

            validation.validate().then(status => {
                if (status === 'Valid') {
                    $.ajax({
                        url: form.attr('action'),
                        data: form.serialize(),
                        method: "POST",
                        success: () => {
                            datatable.reload();
                            Swal.fire("Saved!");
                            $('#newRecordModal').modal('hide');
                        },
                        error: (xhr, result) => {
                            console.log(xhr, result);
                            Swal.fire("Unable to save this item!", xhr.responseJSON.Message, "error");
                        }
                    });
                }
            });
        });
    }
});

function showDeleteModal(row) {
    row = JSON.parse(decodeURIComponent(row));

    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Yes, delete it!"
    }).then(function (result) {
        if (result.value) {
            $.ajax({
                url: HOST_URL + API_PATH + "delete",
                data: {
                    id: row.Id
                },
                method: "POST",
                success: () => {
                    datatable.reload();
                    Swal.fire("Deleted!");
                },
                error: (xhr, result) => {
                    console.log(xhr, result);
                    Swal.fire("Unable to delete this item!", xhr.responseJSON.Message, "error");
                }
            });
        }
    });
}

function exportData(base, source, format, extra) {
    if(typeof extra === "undefined")
        extra = "";
    window.location.href = `${base}?dataSourceName=${source}&format=${format}&filters=${encodeURI(JSON.stringify(datatable.getDataSourceQuery()))}&returnUrl=${window.location.href}&extra=${extra}`
}
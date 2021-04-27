let API_USERS_PATH = "/api/teacher/users_in_class";

function reloadEntries(user) {
    dataContent.find('tbody').empty();
    $.ajax({
        url: HOST_URL + API_LIST_PATH,
        method: 'GET',
        data: {
            studentId: user.Id,
            subjectId: subjectId,
            teacherId: currentUserId
        },
        success: result => {
            dataContent.find('.card.card-custom').removeClass('overlay overlay-block');
            if (typeof handleContentResult !== "undefined")
                handleContentResult(result)
            else {
                result.data.forEach(e => {
                    if (typeof handleEntry !== "undefined")
                        handleEntry(e)
                    let html = $(itemEntryTemplate.render(e));
                    if (e.Semester === 1)
                        dataContent.find('.table-s1').append(html);
                    else if (e.Semester === 2)
                        dataContent.find('.table-s2').append(html);
                });
                if (typeof userExpanded === "function")
                    userExpanded(dataContent);
            }
        }
    });
}

function loadClass(classId) {
    mainCard.addClass('overlay overlay-block');
    let summaryProvider = '';
    if (typeof summaryProviderType !== "undefined")
        summaryProvider = summaryProviderType;
    $.ajax({
        url: HOST_URL + API_USERS_PATH,
        method: "GET",
        data: {
            classId: classId,
            summaryProviderTypeName: summaryProvider,
            subjectId: subjectId
        },
        success: result => {
            mainCard.removeClass('overlay overlay-block');
            studentList.empty();
            $('.all-subjects-menu').toggle(result.IsFormMaster);
            if (!result.IsFormMaster && subjectId === -2 && result.FirstAvailableSubjectId !== -1) {
                // console.log(">> Should switch to ", result.FirstAvailableSubjectId);
                window.location.href = window.location.origin + window.location.pathname + "?subj=" + result.FirstAvailableSubjectId;
            }
            result.Users.forEach(user => {
                let entryHtml = $(itemTemplate.render({
                    id: user.Id,
                    name: user.FullName,
                    summary: user.Summary
                }));
                entryHtml.find('.card-header').on('click', () => {
                    if (entryHtml.find('.card-body-root').is('.show'))
                        return;
                    dataContent.show();
                    dataContent.detach();
                    dataContent.find('.card.card-custom').addClass('overlay overlay-block');
                    dataContent.find('#kt_form_1 input[name="studentId"]').val(user.Id);
                    entryHtml.find('.card-body').append(dataContent);
                    reloadEntries(user);

                });
                studentList.append(entryHtml);
            });
            setupSubmitHook();
        }
    });
}

$(document).ready(() => loadClass(classSelector.val()));
classSelector.on('change', () => loadClass(classSelector.val()));

$('#kt_datatable_search_query').on('change paste keyup', (e) => {
    let search = $(e.target).val().toLowerCase();
    studentList.find('> div').each((i, e1) => {
        e1 = $(e1);
        if (e1.attr('data-name').toLowerCase().includes(search))
            e1.show();
        else
            e1.hide();
    });
});

function setupSubmitHook() {
    dataContent.find('button[type="submit"]').off('click');
    dataContent.find('button[type="submit"]').on('click', e => {
        e.preventDefault();
        let form = dataContent.find('#kt_form_1');
        console.log(form.serialize());
        $.ajax({
            url: dataContent.find('#kt_form_1').attr('action'),
            method: 'POST',
            data: form.serialize(),
            success: result => {
                $(form).parents('.user-root').find('.summary-bar').text(result);

                reloadEntries({
                    Id: $('#kt_form_1 input[name="studentId"]').val()
                });

                showNotification({
                    message: itemName + ' was submitted'
                });

                if (typeof resetForm === "function")
                    resetForm();
            },
            error: (e) => console.log("Error", e)
        });
    });
}

setupSubmitHook();

function deleteGrade(e) {
    let form = $(e).parent('form');
    $.ajax({
        url: form.attr('action'),
        method: 'DELETE',
        data: form.serialize(),
        success: result => {
            form.parents('.user-root').find('.summary-bar').text(result);

            reloadEntries({
                Id: $('#kt_form_1 input[name="studentId"]').val()
            });

            showNotification({
                message: itemName + ' was deleted'
            });
        },
        error: (e) => console.log("Error", e)
    });
}


'use strict';
var initFormModal;

// Thêm mới
$(document).on('click', '.create', function () {

    //Validate clien
    var form = $("#dataForm")[0];
    var formData = new FormData(form);
    var Noloadtable = $(".noloadTable");
    /*form.validate();*/
    if (!form.checkValidity()) {
        $("#dataForm").addClass("was-validated");
        return;
    }

    //if (form.valid()) {

    $.ajax({
        type: 'POST',
        url: $(this).data('url'),
        processData: false,
        contentType: false,
        data: formData,
        success: function (result) {
            //alert('Successfully received Data ');
            if (result != 'false') {
                if (Noloadtable.length == 0) {
                    loadData();
                    MessAlert.AlertAction(true, "create");
                }
                initFormModal.CloseModalDN('#myModal');
                //Toast.Nofication(true, "Cập nhật thành công");
            } else {
                //Toast.Nofication(false, "Cập nhật thất bại");
            }
        },
        error: function (xhr) {
            if (Noloadtable.length > 0) {
                if (xhr.status === 400) {
                    const response = JSON.parse(xhr.responseText);
                    if (response.message != '') {
                        MessAlert.AlertAction(false, "create");
                        //Toast.Nofication(false, response.message)
                    } else {
                        MessAlert.AlertAction(false, "create");
                        //Toast.Nofication(false, "Thêm mới thất bại")
                    }
                }
            } else if (xhr.status === 400 && xhr.responseJSON) {
                const errors = xhr.responseJSON.errors;

                // Reset trước
                $('#dataForm .form-control').removeClass('is-invalid');
                $('#dataForm .invalid-feedback').text('');

                for (const key in errors) {
                    if (errors.hasOwnProperty(key)) {
                        const input = $(`#dataForm [name="${key}"]`);
                        input.addClass('is-invalid');

                        const feedback = input.next('.invalid-feedback');
                        if (feedback.length) {
                            feedback.text(errors[key][0]);
                        }
                    }
                }
            } else {
                MessAlert.AlertAction(false, "create");
                //Toast.Nofication(false, "Thêm mới thất bại")

            }

            //alert('Failed to receive the Data');
        }
    })
    //}
});

//Chỉnh sửa
$(document).on('click', '.edit', function () {
    //Validate clien
    //var form = $("#dataForm");
    var Noloadtable = $(".noloadTable");
    var form = $("#dataForm")[0];
    var formData = new FormData(form);
    //form.validate();
    //form.addClass("was-validated");
    if (!form.checkValidity()) {
        $("#dataForm").addClass("was-validated");
        return;
    }
    //if (form.valid()) {

    $.ajax({
        type: 'POST',
        url: $(this).data('url'),
        processData: false,
        contentType: false,
        //contentType: 'application/x-www-form-urlencoded; charset=UTF-8', // when we use .serialize() this generates the data in query string format. this needs the default contentType (default content type is: contentType: 'application/x-www-form-urlencoded; charset=UTF-8') so it is optional, you can remove it
        //data: $("#dataForm").serialize(),
        data: formData,
        success: function (result) {
            //alert('Successfully received Data ');
            if (result != 'false') {
                if (Noloadtable.length == 0) {
                    loadData();
                    MessAlert.AlertAction(true, "edit");
                }
                initFormModal.CloseModalDN('#myModal');
                //Toast.Nofication(true, "Cập nhật thành công");
            } else {
                //Toast.Nofication(false, "Cập nhật thất bại");
            }

        },
        error: function (xhr) {
            if (Noloadtable.length > 0) {
                if (xhr.status === 400) {
                    const response = JSON.parse(xhr.responseText);
                    if (response.message != '') {
                        MessAlert.AlertAction(false, "create");
                        //Toast.Nofication(false, response.message)
                    } else {
                        MessAlert.AlertAction(false, "create");
                        //Toast.Nofication(false, "Thêm mới thất bại")
                    }
                }
            } else if (xhr.status === 400 && xhr.responseJSON) {
                const errors = xhr.responseJSON.errors;

                // Reset trước
                $('#dataForm .form-control').removeClass('is-invalid');
                $('#dataForm .invalid-feedback').text('');

                for (const key in errors) {
                    if (errors.hasOwnProperty(key)) {
                        const input = $(`#dataForm [name="${key}"]`);
                        input.addClass('is-invalid');

                        const feedback = input.next('.invalid-feedback');
                        if (feedback.length) {
                            feedback.text(errors[key][0]);
                        }
                    }
                }
            } else {
                MessAlert.AlertAction(false, "edit");
            }

        }
    })
    //}
});

//Xóa Function
$(document).on('click', '.delete', function () {
    //var ans = confirm("Are you sure you want to delete this Record?");
    //if (ans) {
    var Noloadtable = $(".noloadTable");
    $.ajax({
        type: "POST",
        url: $(this).data('url'),
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (response) {
            if (Noloadtable.length == 0) {
                loadData();
                MessAlert.AlertAction(true, "delete");
            }
            initFormModal.CloseModalDN('#myModal');
            //Toast.Nofication(response.result, response.message)

        },
        error: function (errormessage) {
            //Toast.Nofication(false, errormessage.responseText)
            //alert('Failed to receive the Data');
            //console.log(errormessage.responseText)
            MessAlert.AlertAction(false, "delete");
        }
    });
    //}
});


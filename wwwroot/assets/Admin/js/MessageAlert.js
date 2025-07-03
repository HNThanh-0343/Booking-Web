var MessAlert = {
    AlertAction: function (succsess, action) {
        var colorSuccsess = "warning";
        var textAction = "";
        switch (action) {
            case "create":
                textAction = "Thêm mới";
                break;
            case "edit":
                textAction = "Chỉnh sửa";
                break;
            case "delete":
                textAction = "Xóa";
                break;
            default:
                break;
        }
        mess = `${textAction} thất bại`;
        if (succsess) {
            colorSuccsess = "success";
            mess = `${textAction} thành công`;
        }

        $("#alertJs").empty().append(`  <div class="alert alert-${colorSuccsess} alert-dismissible text-bg-${colorSuccsess} border-0 fade show" role="alert">
                                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="alert" aria-label="Close"></button>
                                <strong>${mess} </strong>
                            </div>`);
    }
}
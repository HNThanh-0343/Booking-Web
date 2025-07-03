/**
 * TaiKhoan.js - Chức năng quản lý tài khoản người dùng
 * Xử lý đăng nhập, đăng ký, xác thực OTP và quản lý mật khẩu
 */

// Mô-đun chính cho chức năng tài khoản
const AccountManager = (function() {
    // Biến private
    let normalUserOtpVerified = false;
    let partnerUserOtpVerified = false;
    let normalUserOtpCode = "";
    let partnerUserOtpCode = "";
    let recoveryOtpCode = "";
    let recoveryOtpVerified = false;

    // Cache DOM
    const DOM = {};

    /**
     * Khởi tạo chức năng bật/tắt hiển thị mật khẩu cho tất cả các trường mật khẩu
     */
    function initializePasswordToggles() {
        // Xóa các sự kiện click hiện có để tránh trùng lặp
        $(".toggle-password").off("click");
        
        // Thêm các xử lý click mới
        $(".toggle-password").on("click", function() {
            const targetId = $(this).data("target");
            const input = $("#" + targetId);
            const icon = $(this).find("i");
            
            if (input.attr("type") === "password") {
                input.attr("type", "text");
                icon.removeClass("fa-eye").addClass("fa-eye-slash");
            } else {
                input.attr("type", "password");
                icon.removeClass("fa-eye-slash").addClass("fa-eye");
            }
        });
    }
    
    /**
     * Khởi tạo hành vi tập trung (focus) cho form
     */
    function initializeFormFocus() {
        // Tập trung vào trường đầu tiên khi dropdown mở
    $('[data-unfold-target="#signUpDropdown"]').on('click', function() {
        setTimeout(function() {
            $("#signinSrEmail").focus();
            initializePasswordToggles();
        }, 300);
    });
    
        // Focus when switching to registration tab
    $('.js-animation-link[data-target="#signup"]').on('click', function() {
        setTimeout(function() {
            $("#name").focus();
            initializePasswordToggles();
        }, 300);
    });
    
        // Focus when switching to forgot password tab
    $('.js-animation-link[data-target="#forgotPassword"]').on('click', function() {
        setTimeout(function() {
            $("#recoverSrEmail").focus();
            initializePasswordToggles();
        }, 300);
    });
    
        // Focus when switching tabs in registration form
    $('#pills-one-code-sample-tab').on('click', function() {
        setTimeout(function() {
            $("#name").focus();
            initializePasswordToggles();
        }, 300);
    });
    
    $('#pills-two-code-sample-tab').on('click', function() {
        setTimeout(function() {
            $("#pname").focus();
            initializePasswordToggles();
        }, 300);
    });
    }

    /**
     * Khởi tạo xử lý sự kiện bàn phím
     */
    function initializeKeyboardEvents() {
        // Form đăng nhập
    $("#signinSrEmail, #signinSrPassword").on("keyup", function(e) {
        if (e.key === "Enter" || e.keyCode === 13) {
            loginCheck();
        }
    });
    
        // Đăng ký người dùng thường
    $("#name, #signupSrEmail, #phone, #signupSrPassword, #signupSrConfirmPassword, #normalUserOtp").on("keyup", function(e) {
        if (e.key === "Enter" || e.keyCode === 13) {
            registerNormalUser();
        }
    });
    
        // Đăng ký đối tác
    $("#pname, #signupPartnerSrEmail, #pphone, #signupPartnerSrPassword, #signupPartnerSrConfirmPassword").on("keyup", function(e) {
        if (e.key === "Enter" || e.keyCode === 13) {
            registerPartnerUser();
        }
    });
    
        // Xử lý dropdown toàn cục
    $("#signUpDropdown").on("keyup", function(e) {
        if (e.key === "Enter" || e.keyCode === 13) {
            if ($("#login").is(":visible")) {
                loginCheck();
            } else if ($("#signup").is(":visible")) {
                if ($("#pills-one-code-sample").hasClass("active show")) {
                    registerNormalUser();
                } else if ($("#pills-two-code-sample").hasClass("active show")) {
                    registerPartnerUser();
                }
            }
        }
    });
    
        // Form đặt lại mật khẩu - Bước 1
        $("#recoverSrEmail").on("keyup", function(e) {
            if (e.key === "Enter" || e.keyCode === 13) {
                sendRecoveryOtp();
            }
        });
        
        // Form đặt lại mật khẩu - Bước 2
        $("#recoveryOtpCode, #recoveryNewPassword, #recoveryConfirmPassword").on("keyup", function(e) {
            if (e.key === "Enter" || e.keyCode === 13) {
                resetPassword();
            }
        });
        
        // Modal đổi mật khẩu
        $("#currentPassword, #newPassword, #confirmNewPassword").on("keyup", function(e) {
            if (e.key === "Enter" || e.keyCode === 13) {
                submitPasswordChange();
            }
        });
    }

    /**
     * Khởi tạo hành vi xử lý thông báo lỗi
     */
    function initializeErrorHandling() {
        // Ẩn thông báo lỗi đăng nhập khi nhập liệu
    $("#signinSrEmail, #signinSrPassword").on("focus keyup", function() {
        $("#loginErrorContainer").hide();
    });

        // Ẩn thông báo lỗi/thành công đăng ký người dùng thường khi nhập liệu
    $("#name, #signupSrEmail, #phone, #signupSrPassword, #signupSrConfirmPassword").on("focus keyup", function() {
        $("#normalUserErrorContainer").hide();
        $("#normalUserSuccessContainer").hide();
    });

        // Ẩn thông báo lỗi/thành công đăng ký đối tác khi nhập liệu
    $("#pname, #signupPartnerSrEmail, #pphone, #signupPartnerSrPassword, #signupPartnerSrConfirmPassword").on("focus keyup", function() {
        $("#partnerUserErrorContainer").hide();
        $("#partnerUserSuccessContainer").hide();
    });
    
        // Ẩn thông báo đổi mật khẩu khi nhập liệu
    $("#currentPassword, #newPassword, #confirmNewPassword").on("input", function() {
        $("#resetPasswordError").hide();
        $("#resetPasswordSuccess").hide();
    });
    
        // Ẩn thông báo quên mật khẩu khi nhập liệu (bước 1)
    $("#recoverSrEmail").on("input", function() {
        $("#forgotPasswordErrorContainer").hide();
    });
    
    $("#recoveryOtpCode, #recoveryNewPassword, #recoveryConfirmPassword").on("input", function() {
        $("#resetPasswordErrorContainer").hide();
        $("#resetPasswordSuccessContainer").hide();
    });
    }

    /**
     * Bắt đầu đếm ngược thời gian cho OTP
     * @param {string} buttonId - ID của phần tử nút
     * @param {number} seconds - Thời gian đếm ngược (giây)
     * @param {string} countdownElementId - ID tùy chọn của phần tử hiển thị đếm ngược
     */
    function startOtpCountdown(buttonId, seconds, countdownElementId) {
        const button = $("#" + buttonId);
        const originalText = button.text();
        let countdown = seconds;
        
        // Xử lý phần tử hiển thị đếm ngược nếu được cung cấp
        const countdownElement = countdownElementId ? $("#" + countdownElementId) : null;
        if (countdownElement && countdownElement.length > 0) {
            countdownElement.text(countdown);
        }
        
        button.prop("disabled", true);
        
        const interval = setInterval(function() {
            countdown--;
            
            // Cập nhật text đếm ngược
            if (countdownElement && countdownElement.length > 0) {
                countdownElement.text(countdown);
            } else {
                button.text(countdown + "s");
            }
            
            if (countdown < 0) {
                clearInterval(interval);
                button.text(originalText);
                button.prop("disabled", false);
            }
        }, 1000);
    }

    /**
     * Kiểm tra định dạng email hợp lệ
     * @param {string} email - Email cần kiểm tra
     * @returns {boolean} - True nếu hợp lệ
     */
    function validateEmail(email) {
        const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
        return emailPattern.test(email);
    }
    
    /**
     * Kiểm tra xem email có phải là địa chỉ Gmail không
     * @param {string} email - Email cần kiểm tra
     * @returns {boolean} - True nếu là Gmail
     */
    function isGmailAddress(email) {
        return email.toLowerCase().endsWith("@gmail.com");
    }
    
    /**
     * Kiểm tra số điện thoại (chỉ chữ số)
     * @param {string} phone - Số điện thoại cần kiểm tra
     * @returns {boolean} - True nếu hợp lệ
     */
    function validatePhone(phone) {
        // Kiểm tra tất cả các ký tự có phải là chữ số không
        if (!/^\d+$/.test(phone)) {
            return false;
        }
        
        // Kiểm tra độ dài có phải là 10 chữ số không
        if (phone.length !== 10) {
            return false;
        }
        
        // Kiểm tra các tiền tố di động của Việt Nam (03, 05, 07, 08, 09)
        const validPrefixes = ['03', '05', '07', '08', '09'];
        const prefix = phone.substring(0, 2);
        
        return validPrefixes.includes(prefix);
    }
    
    /**
     * Kiểm tra độ mạnh mật khẩu
     * @param {string} password - Mật khẩu cần kiểm tra
     * @returns {boolean} - True nếu hợp lệ
     */
    function validatePassword(password) {
        // Kiểm tra độ dài tối thiểu (6 ký tự)
        return password.length >= 6;
    }
    
    /**
     * Gửi mã OTP đến email người dùng
     * @param {string} email - Email người dùng
     * @param {string} type - Loại người dùng ('normal', 'partner', hoặc 'recovery')
     * @param {Object} elements - Các phần tử UI để hiển thị phản hồi
     */
    function sendOtpCode(email, type, elements) {
        // Kiểm tra email
        if (!email) {
            $(elements.errorMessage).text("Vui lòng nhập email để gửi mã OTP!");
            $(elements.errorContainer).show();
            return;
        }
    
        if (!validateEmail(email)) {
            $(elements.errorMessage).text("Vui lòng nhập email hợp lệ!");
            $(elements.errorContainer).show();
            return;
        }

        if (!isGmailAddress(email)) {
            $(elements.errorMessage).text("Vui lòng sử dụng địa chỉ Gmail.");
            $(elements.errorContainer).show();
            return;
        }
    
        // Update UI
        $(elements.otpMessage).text("Đang gửi mã OTP...").css("color", "#666").show();
        $(elements.sendButton).prop("disabled", true);
        
        // Xác định endpoint dựa trên loại
        const endpoint = type === 'recovery' ? "/TaiKhoan/GuiOtpQuenMatKhau" : "TaiKhoan/GuiOtp";
        const data = type === 'recovery' ? { email } : { email, type };
        
        // Gửi yêu cầu OTP
        $.ajax({
            url: endpoint,
            type: "POST",
            data: data,
            headers: {
                "X-Requested-With": "XMLHttpRequest"
            },
            success: function(res) {
                if (res.success) {
                    // Lưu mã OTP
                    if (type === 'normal') normalUserOtpCode = res.otpCode;
                    else if (type === 'partner') partnerUserOtpCode = res.otpCode;
                    else if (type === 'recovery') {
                        recoveryOtpCode = res.otpCode;
                        // Đối với khôi phục, hiển thị bước 2
                        $("#forgotPasswordStep1").hide();
                        $("#forgotPasswordStep2").show();
                    }
                    
                    // Update UI
                    $(elements.otpMessage).text(res.message).css("color", "green").show();
                    $(elements.otpInput).prop("disabled", false).focus();
                    
                    // Bắt đầu đếm ngược
                    if (type === 'recovery') {
                        const resendBtn = $("#resendRecoveryOtpBtn");
                        const countdownSpan = $("#recoveryOtpCountdown");
                        let secondsLeft = 10;
                        
                        resendBtn.prop("disabled", true);
                        countdownSpan.text(secondsLeft);
                        
                        const countdownInterval = setInterval(function() {
                            secondsLeft--;
                            countdownSpan.text(secondsLeft);
                            
                            if (secondsLeft <= 0) {
                                clearInterval(countdownInterval);
                                resendBtn.prop("disabled", false);
                                resendBtn.removeClass("text-muted").addClass("text-primary");
                                resendBtn.html('Gửi lại mã OTP');
                            }
                        }, 1000);
                    } else {
                        startOtpCountdown(elements.sendButton.substring(1), 60);
                    }
                    
                    // Thiết lập kiểm tra OTP
                    $(elements.otpInput).off("input").on("input", function() {
                        const enteredOtp = $(this).val().trim();
                        if (enteredOtp.length === 6) {
                            let correctOtp;
                            let otpVerified = false;
                            
                            if (type === 'normal') {
                                correctOtp = normalUserOtpCode;
                                normalUserOtpVerified = enteredOtp === correctOtp;
                                otpVerified = normalUserOtpVerified;
                            } else if (type === 'partner') {
                                correctOtp = partnerUserOtpCode;
                                partnerUserOtpVerified = enteredOtp === correctOtp;
                                otpVerified = partnerUserOtpVerified;
                            } else if (type === 'recovery') {
                                correctOtp = recoveryOtpCode;
                                recoveryOtpVerified = enteredOtp === correctOtp;
                                otpVerified = recoveryOtpVerified;
                                $("#resetPasswordBtn").prop("disabled", !otpVerified);
                            }
                            
                            // Cập nhật UI dựa trên kết quả xác thực
                            const message = otpVerified ? "Xác thực OTP thành công!" : "Mã OTP không chính xác!";
                            const color = otpVerified ? "green" : "red";
                            $(elements.otpMessage).text(message).css("color", color).show();
                        } else if (type === 'recovery') {
                            recoveryOtpVerified = false;
                            $("#resetPasswordBtn").prop("disabled", false);
                        }
                    });
                } else {
                    $(elements.errorMessage).text(res.message || "Không thể gửi mã OTP. Vui lòng thử lại.");
                    $(elements.errorContainer).show();
                    $(elements.sendButton).prop("disabled", false);
                }
            },
            error: function() {
                $(elements.errorMessage).text("Đã xảy ra lỗi khi gửi mã OTP. Vui lòng thử lại.");
                $(elements.errorContainer).show();
                $(elements.sendButton).prop("disabled", false);
            }
        });
    }

    /**
     * Cập nhật UI với thông tin người dùng sau khi xác thực thành công
     * @param {Object} userData - Đối tượng dữ liệu người dùng
     */
    function updateUserInterface(userData) {
        console.log("Login successful, reloading page to show role-based UI...");
        
        // Show message indicating successful login and page reload
        const message = `<h class="alert alert-success">Đăng nhập thành công!</h>`;
        $("#loginErrorContainer").hide();
        $("#loginSuccessContainer").html(message).show();
        
        // Force an immediate page reload
        setTimeout(function() {
            // Use hard reload to ensure full page refresh
            window.location.href = window.location.href;
        }, 800);
    }

    // Phương thức public
    return {
        init: function() {
            // Thêm style cursor pointer
            $("<style>.cursor-pointer { cursor: pointer; }</style>").appendTo("head");
            
            // Khởi tạo tất cả các thành phần
            initializePasswordToggles();
            initializeFormFocus();
            initializeKeyboardEvents();
            initializeErrorHandling();
        },
        
        // Gửi OTP cho đăng ký người dùng thường
        sendNormalUserOtp: function() {
            const email = $("#signupSrEmail").val().trim();
            sendOtpCode(email, 'normal', {
                errorMessage: "#normalUserErrorMessage",
                errorContainer: "#normalUserErrorContainer",
                otpMessage: "#normalUserOtpMessage",
                sendButton: "#normalUserSendOtp",
                otpInput: "#normalUserOtp"
            });
        },
        
        // Gửi OTP cho đăng ký đối tác
        sendPartnerUserOtp: function() {
            const email = $("#signupPartnerSrEmail").val().trim();
            sendOtpCode(email, 'partner', {
                errorMessage: "#partnerUserErrorMessage",
                errorContainer: "#partnerUserErrorContainer",
                otpMessage: "#partnerUserOtpMessage",
                sendButton: "#partnerUserSendOtp",
                otpInput: "#partnerUserOtp"
            });
        },
        
        // Gửi OTP cho quên mật khẩu
        sendRecoveryOtp: function() {
            const email = $("#recoverSrEmail").val().trim();
            sendOtpCode(email, 'recovery', {
                errorMessage: "#forgotPasswordErrorMessage",
                errorContainer: "#forgotPasswordErrorContainer",
                otpMessage: "#recoveryOtpMessage",
                sendButton: "#sendRecoveryOtpBtn", 
                otpInput: "#recoveryOtpCode"
            });
        },
        
        // Đăng ký người dùng thường
        registerNormalUser: function() {
            // Ngăn gửi lại trùng lặp
            if ($("#normalUserRegisterBtn").hasClass("processing")) {
                return;
            }

            // Kiểm tra dữ liệu nhập
            var name = $("#name").val().trim();
            var email = $("#signupSrEmail").val().trim();
            var phone = $("#phone").val().trim();
            var password = $("#signupSrPassword").val();
            var confirmPassword = $("#signupSrConfirmPassword").val();
            var otpCode = $("#normalUserOtp").val().trim();
            var agreeTerms = $("#customCheckboxInline2").is(":checked");

            // Xóa thông báo lỗi và thành công trước đó
            $("#normalUserErrorContainer, #normalUserSuccessContainer").hide();
            
            // Kiểm tra các trường bắt buộc
            if (!name || !email || !phone || !password || !confirmPassword) {
                $("#normalUserErrorMessage").text("Vui lòng điền đầy đủ thông tin đăng ký.");
                $("#normalUserErrorContainer").show();
                return;
            }

            // Kiểm tra định dạng số điện thoại
            if (!validatePhone(phone)) {
                $("#normalUserErrorMessage").text("Số điện thoại không hợp lệ. Số điện thoại phải có 10 chữ số và bắt đầu bằng đầu số Việt Nam (03, 05, 07, 08, 09).");
                $("#normalUserErrorContainer").show();
                return;
            }

            // Kiểm tra độ dài mật khẩu
            if (!validatePassword(password)) {
                $("#normalUserErrorMessage").text("Mật khẩu phải có ít nhất 6 ký tự.");
                $("#normalUserErrorContainer").show();
                return;
            }

            // Kiểm tra mật khẩu khớp
            if (password !== confirmPassword) {
                $("#normalUserErrorMessage").text("Mật khẩu và xác nhận mật khẩu không khớp.");
                $("#normalUserErrorContainer").show();
                return;
            }

            // Kiểm tra đồng ý điều khoản
            if (!agreeTerms) {
                $("#normalUserErrorMessage").text("Vui lòng chấp nhận điều khoản và chính sách bảo mật.");
                $("#normalUserErrorContainer").show();
                return;
            }
    
            // Kiểm tra xem OTP đã được xác thực chưa
            if (!normalUserOtpVerified && !otpCode) {
                $("#normalUserErrorMessage").text("Vui lòng xác thực email bằng mã OTP trước khi đăng ký.");
                $("#normalUserErrorContainer").show();
                return;
            }

            // Đặt trạng thái đang xử lý
            $("#normalUserRegisterBtn").addClass("processing").prop("disabled", true).html('<i class="fas fa-spinner fa-spin mr-2"></i>Đang xử lý...');
            
            // Gửi yêu cầu đăng ký
            $.ajax({
            url: "/TaiKhoan/DangKyNormalUser",
            type: "POST",
            data: {
            name: name,
            email: email,
            phone: phone,
            password: password,
            otpCode: otpCode
            },
            headers: {
                "X-Requested-With": "XMLHttpRequest"
            },
            success: function(res) {
            if (res.success) {
                    // Hiển thị thông báo thành công
                    $("#normalUserSuccessMessage").text(res.message || "Đăng ký thành công!");
                    $("#normalUserSuccessContainer").show();
                    $("#normalUserErrorContainer").hide();
                        
                        // Cập nhật UI với dữ liệu người dùng nếu được cung cấp
                        if (res.data) {
                            try {
                                var userData = JSON.parse(res.data);
                                // Cập nhật UI và reload trang ngay lập tức
                                updateUserInterface(userData);
                            } catch (e) {
                                console.error("Error parsing user data:", e);
                                // Nếu phân tích thất bại, vẫn reload trang
                                window.location.reload();
                            }
                        } else {
                            // Nếu không có dữ liệu người dùng, chỉ reload trang
                            window.location.reload();
                        }
            } else {
                        $("#normalUserErrorMessage").text(res.message || "Đăng ký không thành công.");
                $("#normalUserErrorContainer").show();
                        $("#normalUserSuccessContainer").hide();
            }
        },
                error: function(xhr, status, error) {
                    $("#normalUserErrorMessage").text("Đã xảy ra lỗi khi đăng ký: " + error);
            $("#normalUserErrorContainer").show();
                    $("#normalUserSuccessContainer").hide();
                },
                complete: function() {
                    // Reset trạng thái nút sau một khoảng trễ để ngăn gửi lại ngay lập tức
                    setTimeout(function() {
                        $("#normalUserRegisterBtn").removeClass("processing").prop("disabled", false).text("Đăng ký");
                    }, 1000);
                }
            });
        },
        
        // Đăng ký đối tác
        registerPartnerUser: function() {
            // Ngăn gửi lại trùng lặp
            if ($("#partnerUserRegisterBtn").hasClass("processing")) {
        return;
    }
    
            // Kiểm tra dữ liệu nhập
            var name = $("#pname").val().trim();
            var email = $("#signupPartnerSrEmail").val().trim();
            var phone = $("#pphone").val().trim();
            var password = $("#signupPartnerSrPassword").val();
            var confirmPassword = $("#signupPartnerSrConfirmPassword").val();
            var otpCode = $("#partnerUserOtp").val().trim();
            var agreeTerms = $("#customCheckboxInline3").is(":checked");

            // Xóa thông báo lỗi và thành công trước đó
            $("#partnerUserErrorContainer, #partnerUserSuccessContainer").hide();
            
            // Kiểm tra các trường bắt buộc
            if (!name || !email || !phone || !password || !confirmPassword) {
                $("#partnerUserErrorMessage").text("Vui lòng điền đầy đủ thông tin đăng ký.");
        $("#partnerUserErrorContainer").show();
        return;
    }
    
            // Kiểm tra định dạng số điện thoại
            if (!validatePhone(phone)) {
                $("#partnerUserErrorMessage").text("Số điện thoại không hợp lệ. Số điện thoại phải có 10 chữ số và bắt đầu bằng đầu số Việt Nam (03, 05, 07, 08, 09).");
        $("#partnerUserErrorContainer").show();
        return;
    }

            // Kiểm tra độ mạnh mật khẩu
            if (!validatePassword(password)) {
                $("#partnerUserErrorMessage").text("Mật khẩu phải có ít nhất 6 ký tự.");
        $("#partnerUserErrorContainer").show();
        return;
    }

            // Kiểm tra mật khẩu khớp
            if (password !== confirmPassword) {
                $("#partnerUserErrorMessage").text("Mật khẩu và xác nhận mật khẩu không khớp.");
        $("#partnerUserErrorContainer").show();
        return;
    }

            // Kiểm tra đồng ý điều khoản
    if (!agreeTerms) {
                $("#partnerUserErrorMessage").text("Vui lòng chấp nhận điều khoản và chính sách bảo mật.");
        $("#partnerUserErrorContainer").show();
        return;
    }
    
            // Kiểm tra xem OTP đã được xác thực chưa
            if (!partnerUserOtpVerified && !otpCode) {
                $("#partnerUserErrorMessage").text("Vui lòng xác thực email bằng mã OTP trước khi đăng ký.");
        $("#partnerUserErrorContainer").show();
        return;
    }

            // Đặt trạng thái đang xử lý
            $("#partnerUserRegisterBtn").addClass("processing").prop("disabled", true).html('<i class="fas fa-spinner fa-spin mr-2"></i>Đang xử lý...');
            
            // Gửi yêu cầu đăng ký
    $.ajax({
                url: "/TaiKhoan/DangKyPartnerUser",
        type: "POST",
        data: {
                    pname: name,
                    pemail: email,
                    pphone: phone,
                    Partnerpassword: password,
                    otpCode: otpCode
        },
        headers: {
            "X-Requested-With": "XMLHttpRequest"
        },
        success: function(res) {
            if (res.success) {
                        // Hiển thị thông báo thành công
                        $("#partnerUserSuccessMessage").text(res.message || "Đăng ký thành công!");
                        $("#partnerUserSuccessContainer").show();
                        $("#partnerUserErrorContainer").hide();
                        
                        // Cập nhật UI với dữ liệu người dùng nếu được cung cấp
                        if (res.data) {
                            try {
                                var userData = JSON.parse(res.data);
                                // Cập nhật UI và reload trang ngay lập tức
                                updateUserInterface(userData);
                            } catch (e) {
                                console.error("Error parsing user data:", e);
                                // Nếu phân tích thất bại, vẫn reload trang
                                window.location.reload();
                            }
                        } else {
                            // Nếu không có dữ liệu người dùng, chỉ reload trang
                            window.location.reload();
                        }
            } else {
                        $("#partnerUserErrorMessage").text(res.message || "Đăng ký không thành công.");
                        $("#partnerUserErrorContainer").show();
                        $("#partnerUserSuccessContainer").hide();
            }
        },
                error: function(xhr, status, error) {
                    $("#partnerUserErrorMessage").text("Đã xảy ra lỗi khi đăng ký: " + error);
                    $("#partnerUserErrorContainer").show();
                    $("#partnerUserSuccessContainer").hide();
                },
                complete: function() {
                    // Reset trạng thái nút sau một khoảng trễ
                    setTimeout(function() {
                        $("#partnerUserRegisterBtn").removeClass("processing").prop("disabled", false).text("Đăng ký");
                    }, 1000);
                }
            });
        },

        // Đăng nhập người dùng
        loginCheck: function() {
            // Ngăn gửi lại trùng lặp
            if ($("#loginButton").hasClass("processing")) {
                return;
            }

            var email = $("#signinSrEmail").val().trim();
            var password = $("#signinSrPassword").val();

            // Xóa các thông báo trước đó
            $("#loginErrorContainer, #loginSuccessContainer").hide();

            // Kiểm tra
    if (!email || !password) {
                $("#loginErrorMessage").text("Vui lòng nhập đầy đủ email và mật khẩu.");
                $("#loginErrorContainer").show();
        return;
    }

            // Đặt trạng thái đang xử lý
            $("#loginButton").addClass("processing").prop("disabled", true).html('<i class="fas fa-spinner fa-spin mr-2"></i>Đang xử lý...');

    $.ajax({
                url: "/TaiKhoan/DangNhap",
        type: "POST",
        data: {
            email: email,
            matKhau: password
        },
        headers: {
            "X-Requested-With": "XMLHttpRequest"
        },
        success: function(res) {
            if (res.success) {
                        // Hiển thị thông báo thành công
                        $("#loginSuccessMessage").text(res.message || "Đăng nhập thành công!");
                        $("#loginSuccessContainer").show();
                        $("#loginErrorContainer").hide();
                        
                        // Cập nhật UI với dữ liệu người dùng nếu được cung cấp
                        if (res.data) {
                            try {
                                var userData = JSON.parse(res.data);
                                // Cập nhật UI và reload trang ngay lập tức
                                updateUserInterface(userData);
                            } catch (e) {
                                console.error("Error parsing user data:", e);
                                // Nếu phân tích thất bại, vẫn reload trang
                                window.location.reload();
                            }
                        } else {
                            // Nếu không có dữ liệu người dùng, chỉ reload trang
                            window.location.reload();
                        }
            } else {
                        $("#loginErrorMessage").text(res.message || "Đăng nhập không thành công.");
                        $("#loginErrorContainer").show();
                        $("#loginSuccessContainer").hide();
            }
        },
                error: function(xhr, status, error) {
                    $("#loginErrorMessage").text("Đã xảy ra lỗi khi đăng nhập: " + error);
                    $("#loginErrorContainer").show();
                    $("#loginSuccessContainer").hide();
                },
                complete: function() {
                    // Reset trạng thái nút sau một khoảng trễ
                    setTimeout(function() {
                        $("#loginButton").removeClass("processing").prop("disabled", false).text("Đăng nhập");
                    }, 1000);
                }
            });
        },
        
        // Hiển thị form quên mật khẩu
        showForgotPasswordForm: function() {
            // Đóng dropdown người dùng
    $('#userDropdownInvoker').trigger('click');
            // Hiển thị modal đặt lại mật khẩu
    $('#passwordResetModal').modal('show');
        },

        // Hiển thị form khôi phục mật khẩu trong dropdown đăng nhập
        showRecoveryForm: function() {
    $('.js-animation-link[data-target="#forgotPassword"]').click();
        },
        
        // Gửi yêu cầu đổi mật khẩu
        submitPasswordChange: function() {
            const currentPassword = $("#currentPassword").val().trim();
            const newPassword = $("#newPassword").val().trim();
            const confirmNewPassword = $("#confirmNewPassword").val().trim();
        
            // Ẩn các thông báo lỗi trước đó
    $("#resetPasswordError").hide();

            // Kiểm tra dữ liệu nhập
    if (!currentPassword) {
        $("#resetPasswordError").text("Vui lòng nhập mật khẩu hiện tại!").show();
        return;
    }

    if (!newPassword) {
        $("#resetPasswordError").text("Vui lòng nhập mật khẩu mới!").show();
        return;
    }

    if (!confirmNewPassword) {
        $("#resetPasswordError").text("Vui lòng xác nhận mật khẩu mới!").show();
        return;
    }

    if (newPassword !== confirmNewPassword) {
        $("#resetPasswordError").text("Mật khẩu mới và xác nhận mật khẩu không khớp!").show();
        return;
    }

            // Gửi yêu cầu đổi mật khẩu
    $.ajax({
        url: "/TaiKhoan/DoiMatKhau",
        type: "POST",
        data: { 
            currentPassword: currentPassword,
            newPassword: newPassword,
            confirmPassword: confirmNewPassword
        },
        headers: {
            "X-Requested-With": "XMLHttpRequest"
        },
        success: function(res) {
            if (res.success) {
                        // Hiển thị thông báo thành công
                $("#resetPasswordError").hide();
                $("#resetPasswordSuccessMessage").text("Mật khẩu của bạn đã được thay đổi thành công.");
                $("#resetPasswordSuccess").show();
                
                        // Xóa các trường form
                $("#currentPassword, #newPassword, #confirmNewPassword").val("");
                
                        // Đóng modal sau một khoảng trễ
                setTimeout(function() {
                    $("#passwordResetModal").modal('hide');
                }, 1500);
            } else {
                $("#resetPasswordSuccess").hide();
                $("#resetPasswordError").text(res.message || "Có lỗi xảy ra khi đổi mật khẩu.").show();
            }
        },
        error: function() {
            $("#resetPasswordError").text("Đã xảy ra lỗi. Vui lòng thử lại sau.").show();
        }
    });
        },
        
        // Đặt lại mật khẩu với OTP
        resetPassword: function() {
            // Ẩn các container thông báo
    $("#resetPasswordErrorContainer").hide();
    $("#resetPasswordSuccessContainer").hide();
    
            // Lấy giá trị từ form
            const email = $("#recoverSrEmail").val().trim();
            const otpCode = $("#recoveryOtpCode").val().trim();
            const newPassword = $("#recoveryNewPassword").val().trim();
            const confirmPassword = $("#recoveryConfirmPassword").val().trim();
            
            // Kiểm tra dữ liệu nhập
    if (!otpCode) {
        $("#resetPasswordErrorMessage").text("Vui lòng nhập mã OTP!");
        $("#resetPasswordErrorContainer").show();
        return;
    }
    
    if (!newPassword) {
        $("#resetPasswordErrorMessage").text("Vui lòng nhập mật khẩu mới!");
        $("#resetPasswordErrorContainer").show();
        return;
    }
    
    if (!confirmPassword) {
        $("#resetPasswordErrorMessage").text("Vui lòng xác nhận mật khẩu mới!");
        $("#resetPasswordErrorContainer").show();
        return;
    }
    
    // Kiểm tra độ dài mật khẩu
    if (newPassword.length < 6) {
        $("#resetPasswordErrorMessage").text("Mật khẩu phải có ít nhất 6 ký tự.");
        $("#resetPasswordErrorContainer").show();
        $("#recoveryNewPassword").focus();
        return;
    }
    
    if (newPassword !== confirmPassword) {
        $("#resetPasswordErrorMessage").text("Mật khẩu mới và xác nhận mật khẩu không khớp!");
        $("#resetPasswordErrorContainer").show();
        return;
    }
    
    $("#resetPasswordBtn").prop("disabled", true).text("Đang xử lý...");
    
            // Gửi yêu cầu đặt lại mật khẩu
    $.ajax({
        url: "/TaiKhoan/QuenMatKhau",
        type: "POST",
        data: {
            email: email,
            otpCode: otpCode,
            newPassword: newPassword,
            confirmPassword: confirmPassword
        },
        headers: {
            "X-Requested-With": "XMLHttpRequest"
        },
        success: function(res) {
                    // Bật lại nút
            $("#resetPasswordBtn").prop("disabled", false).text("Đặt lại mật khẩu");
            
            if (res.success) {
                        // Hiển thị thông báo thành công
                $("#resetPasswordSuccessMessage").text(res.message || "Mật khẩu đã được cập nhật thành công.");
                $("#resetPasswordSuccessContainer").show();
                
                        // Xóa các trường form
                $("#recoveryOtpCode, #recoveryNewPassword, #recoveryConfirmPassword").val("");
                
                        // Chuyển hướng đến form đăng nhập sau một khoảng trễ
                setTimeout(function() {
                            // Chuyển sang form đăng nhập
                    $('.js-animation-link[data-target="#login"]').click();
                    
                            // Đặt lại form khôi phục mật khẩu về bước 1
                    $("#forgotPasswordStep2").hide();
                    $("#forgotPasswordStep1").show();
                    $("#recoverSrEmail").val("");
                    
                            // Xóa tất cả các thông báo
                    $("#forgotPasswordErrorContainer, #resetPasswordErrorContainer, #resetPasswordSuccessContainer, #recoveryOtpMessage").hide();
                }, 2000);
            } else {
                $("#resetPasswordErrorMessage").text(res.message || "Có lỗi xảy ra khi đặt lại mật khẩu.");
                $("#resetPasswordErrorContainer").show();
                
                // Xóa trường OTP khi kiểm tra thất bại
                $("#recoveryOtpCode").val("");
                $("#recoveryOtpCode").focus();
            }
        },
        error: function() {
                    // Bật lại nút
            $("#resetPasswordBtn").prop("disabled", false).text("Đặt lại mật khẩu");
            
            $("#resetPasswordErrorMessage").text("Đã xảy ra lỗi. Vui lòng thử lại sau.");
            $("#resetPasswordErrorContainer").show();
            
            // Xóa trường OTP khi xảy ra lỗi
            $("#recoveryOtpCode").val("");
            $("#recoveryOtpCode").focus();
        }
    });
        },

        // Quay lại bước 1 trong khôi phục mật khẩu
        backToStep1: function() {
    $("#forgotPasswordStep2").hide();
    $("#forgotPasswordStep1").show();
    $("#sendRecoveryOtpBtn").prop("disabled", false).text("Gửi mã xác thực");
    $("#forgotPasswordErrorContainer").hide();
        }
    };
})();

// Khởi tạo khi document đã sẵn sàng
$(document).ready(function() {
    AccountManager.init();
    
    // Thêm xử lý sự kiện để xóa thông báo lỗi khi nhập OTP thay đổi
    $("#recoveryOtpCode").on("input", function() {
        $("#resetPasswordErrorContainer").hide();
    });
    
    // Xóa thông báo lỗi khi nhập mật khẩu thay đổi cho đặt lại mật khẩu
    $("#recoveryNewPassword, #recoveryConfirmPassword").on("input", function() {
        $("#resetPasswordErrorContainer").hide();
    });
});

// Hiển thị các phương thức public cần thiết cho HTML
function sendNormalUserOtp() {
    AccountManager.sendNormalUserOtp();
}

function sendPartnerUserOtp() {
    AccountManager.sendPartnerUserOtp();
}

function registerNormalUser() {
    AccountManager.registerNormalUser();
}

function registerPartnerUser() {
    AccountManager.registerPartnerUser();
}

function loginCheck() {
    AccountManager.loginCheck();
}

function showForgotPasswordForm() {
    AccountManager.showForgotPasswordForm();
}

function showRecoveryForm() {
    AccountManager.showRecoveryForm();
}

function submitPasswordChange() {
    AccountManager.submitPasswordChange();
}

function sendRecoveryOtp() {
    AccountManager.sendRecoveryOtp();
}

function resetPassword() {
    AccountManager.resetPassword();
}

function backToStep1() {
    AccountManager.backToStep1();
} 
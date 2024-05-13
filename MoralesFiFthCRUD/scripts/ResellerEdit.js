$(document).ready(function () {
    $("#editBtn").click(function () {
       
        $("#first_name, #last_name, #phone, #email, #location").prop("disabled", false);

      
        $("#SaveBtn, #resetBtn").show();
        $("#editBtn").hide();
    });

    $("#SaveBtn").click(function () {
       
        $("#registrationForm").submit();
    });

    $("#resetBtn").click(function () {
       
        $("#first_name, #last_name, #phone, #email, #location").prop("disabled", true);

       
        $("#first_name").val("@Model.Firstname");
        $("#last_name").val("@Model.Lastname");
        
        $("#editBtn").show();
        $("#SaveBtn, #resetBtn").hide();
    });

   
    $("#togglePassword").click(function () {
        var passwordField = $("#password");
        var passwordFieldType = passwordField.attr('type');

        if (passwordFieldType === 'password') {
            passwordField.attr('type', 'text');
            $(this).text("Hide"); 
        } else {
            passwordField.attr('type', 'password');
            $(this).text("Show"); 
        }
    });
});

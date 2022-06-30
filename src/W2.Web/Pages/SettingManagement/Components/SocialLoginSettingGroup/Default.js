$(function () {
    $("#SocialLoginSettingForm").on("submit", function (event) {
        event.preventDefault();
        const form = $(this).serializeFormToObject();
        console.log(form);
        w2.settings.setting.updateSocialLoginSettings(form)
            .then(function (result) {
                $(document).trigger("AbpSettingSaved");
            });
    });
});
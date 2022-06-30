//const token = $('input[name="__RequestVerificationToken"]').val();
//const elsaStudioRoot = document.querySelector('elsa-studio-root');

//elsaStudioRoot.addEventListener('HttpClientCreated', event => {

//    // Get access to the axios middleware service and register a sample middleware:
//    const { service } = event.detail;

//    service.register({
//        onRequest(request) {
//            console.log(request);
//            request.headers = {
//                ...request.headers,
//                'RequestVerificationToken': token
//            };
//            return request;
//        },

//        onResponse(response) {
//            return response;
//        }
//    });

//});
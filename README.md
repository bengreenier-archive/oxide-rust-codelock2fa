# oxide-rust-codelock2fa

> master-service is the master branch for the [rust2fa](https://rust2fa.azurewebsites.net) site.

A nodejs service to facilitate communications between the `oxide-rust-codelock2fa` plugin and twilio (to communicate with players phones).

# Setup


## Twilio

+ Register for twilio programatic sms
+ Get a number
+ Configure twilio request url to that of your service (ie: `https://rust2fa.azurewebsites.net/2fa`)

## Service

+ Install [nodejs](https://nodejs.org)
+ clone this repo @ `master-service`
+ `npm install`
+ set environment variables `TWILIO_NUMBER, TWILIO_SID, TWILIO_TOKEN` and optionally `PORT`
+ `npm start`

# License

MIT

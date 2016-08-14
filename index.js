const fs = require('fs')
const express = require('express')
const baseball = require('baseball')
const twilio = require('twilio')
const bodyParser = require('body-parser')
const serveStatic = require('serve-static')

baseball("TWILIO_NUMBER")
baseball("TWILIO_SID")
baseball("TWILIO_TOKEN")

const client = twilio(process.env.TWILIO_SID, process.env.TWILIO_TOKEN)
const app = express()

// TODO: lru cache or similar
let messageQueue = []

// TODO: db or similar
let userMap = {
    "76561198051009569": "+16034405544"
}

// serve public/
app.use(serveStatic('public'))

app.get("/", (req, res) => {
    res.sendFile("public/index.html")
})

// called by game plugin
app.get("/2fa", (req, res) => {
    let door = req.query.door
    let player = req.query.player
    
    if (!door) {
        return res.send(400, {error: "missing door querystring"})
    }
    if (!player) {
        return res.send(400, {error: "missing player querystring"})
    }

    let id = userMap[player];

    // no user found, need to register
    if (!id) {
        return res.send(404, {error: "no such user"})
    }

    client.sendMessage({
        to: id,
        from: process.env.TWILIO_NUMBER,
        body: `Authorize access to door at ${door} ?`
    }, (err, tRes) => {
        if (err) {
            return res.send(500, {error: "unable to send text"})
        } else {
            let pollInterval = setInterval(() => {
                if (messageQueue.indexOf(id) > -1) {
                    messageQueue = messageQueue.filter(v => v != id)

                    // we got ack!
                    res.send(200)
                }
            }, 1 * 1000)
            setTimeout(() => {
                clearInterval(pollInterval)
                res.send(401, {error: "no response from user"})
            }, 60 * 1000)
            
        }
    })
})

// called by twilio response
app.post("/2fa", bodyParser.urlencoded({extended: true}), (req, res, next) => {
    if (twilio.validateExpressRequest(req, process.env.TWILIO_TOKEN)) {
        next()
    } else {
        return res.send(403, {error: "you aren't twilio"})
    }
}, (req, res) => {
    let msg = req.body

    // add number to queue if body looks good
    if (msg.Body.toLowerCase().indexOf("no") == -1) {
        messageQueue.push(msg.From)
    }

    // thank twilio
    res.send(200)
})

// called by user on first run
app.post("/new", bodyParser.urlencoded({extended: true}), (req, res) => {
    let steam = req.body.steam
    let mobile = req.body.mobile

    if (!steam) {
        return res.send(400, {error: "missing steam body param"})
    }
    if (!mobile) {
        return res.send(400, {error: "missing mobile body param"})
    }

    // TODO: sanity check
    userMap[steam] = "+"+ mobile

    res.send(200)
})

// startup
let server = app.listen(process.env.PORT || 3001, () => console.log("up on " + server.address().port))
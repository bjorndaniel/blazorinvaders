module.exports = async function (context, req, highScore) {
    context.log('Fetching current high score');
    if (!req.query.id) {
        context.res = {
            status: 401
        };
    }
    else {
        context.log(req.query.id);
        context.bindings.saveConnection = JSON.stringify({
            id: req.query.id,
        });
        context.res = {
            // status: 200, /* Defaults to 200 */
            body: highScore
        };
    }

}

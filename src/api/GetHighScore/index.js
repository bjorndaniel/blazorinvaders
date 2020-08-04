module.exports = async function (context, req, highScore) {
    context.log('Fetching current high score');
    context.res = {
        // status: 200, /* Defaults to 200 */
        body: highScore
    };
}
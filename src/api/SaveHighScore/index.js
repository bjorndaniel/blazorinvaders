module.exports = async function (context, req) {
    context.log(`Saving high score for ${req.body.name}`);
    context.bindings.saveHighScore = JSON.stringify({
        id: 'CurrentHighScore',
        name: req.body.name,
        score: req.body.score
    });

    context.res = {
        // status: 200, /* Defaults to 200 */
        body: "Saved high score"
    };
}
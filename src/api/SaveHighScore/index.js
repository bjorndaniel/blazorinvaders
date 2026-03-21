module.exports = async function (context, req) {
    if (!req.body || !req.body.name || req.body.score === undefined) {
        context.res = { status: 400, body: 'Missing name or score' };
        return;
    }
    context.bindings.highScoreOut = {
        PartitionKey: 'HighScore',
        RowKey: 'Current',
        Name: req.body.name,
        Score: req.body.score
    };
    context.res = { body: 'Saved high score' };
};

module.exports = async function (context, req, highScoreEntity) {
    if (!highScoreEntity) {
        context.res = { body: null };
        return;
    }
    context.res = {
        body: {
            name: highScoreEntity.Name,
            score: highScoreEntity.Score
        }
    };
};

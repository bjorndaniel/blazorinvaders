const { TableClient } = require('@azure/data-tables');

module.exports = async function (context, req) {
    const connectionString = process.env.AzureWebJobsStorage;
    if (!connectionString) {
        context.res = { status: 500, body: 'Storage not configured' };
        return;
    }
    try {
        const client = TableClient.fromConnectionString(connectionString, 'HighScores');
        const entity = await client.getEntity('HighScore', 'Current');
        context.res = {
            body: { name: entity.Name, score: entity.Score }
        };
    } catch (err) {
        if (err.statusCode === 404) {
            context.res = { body: null };
        } else {
            context.res = { status: 500, body: err.message };
        }
    }
};

const { TableClient } = require('@azure/data-tables');

module.exports = async function (context, req) {
    if (!req.body || !req.body.name || req.body.score === undefined) {
        context.res = { status: 400, body: 'Missing name or score' };
        return;
    }
    const connectionString = process.env.AzureWebJobsStorage;
    if (!connectionString) {
        context.res = { status: 500, body: 'Storage not configured' };
        return;
    }
    try {
        const client = TableClient.fromConnectionString(connectionString, 'HighScores');
        try { await client.createTable(); } catch { /* already exists */ }
        await client.upsertEntity({
            partitionKey: 'HighScore',
            rowKey: 'Current',
            Name: req.body.name,
            Score: req.body.score
        }, 'Replace');
        context.res = { body: 'Saved high score' };
    } catch (err) {
        context.res = { status: 500, body: err.message };
    }
};

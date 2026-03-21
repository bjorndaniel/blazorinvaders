const { app } = require('@azure/functions');
const { TableClient } = require('@azure/data-tables');

app.http('savehighscore', {
    methods: ['POST'],
    authLevel: 'anonymous',
    handler: async (request, context) => {
        const body = await request.json();
        if (!body || !body.name || body.score === undefined) {
            return { status: 400, body: 'Missing name or score' };
        }
        const connectionString = process.env.AzureWebJobsStorage;
        if (!connectionString) {
            return { status: 500, body: 'Storage not configured' };
        }
        try {
            const client = TableClient.fromConnectionString(connectionString, 'HighScores');
            try { await client.createTable(); } catch { /* already exists */ }
            await client.upsertEntity({
                partitionKey: 'HighScore',
                rowKey: 'Current',
                Name: body.name,
                Score: body.score
            }, 'Replace');
            return { body: 'Saved high score' };
        } catch (err) {
            return { status: 500, body: err.message };
        }
    }
});

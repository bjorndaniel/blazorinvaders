const { app } = require('@azure/functions');
const { TableClient } = require('@azure/data-tables');

app.http('gethighscore', {
    methods: ['GET'],
    authLevel: 'anonymous',
    handler: async (request, context) => {
        const connectionString = process.env.AzureWebJobsStorage;
        if (!connectionString) {
            return { status: 500, body: 'Storage not configured' };
        }
        try {
            const client = TableClient.fromConnectionString(connectionString, 'HighScores');
            const entity = await client.getEntity('HighScore', 'Current');
            return { jsonBody: { name: entity.Name, score: entity.Score } };
        } catch (err) {
            if (err.statusCode === 404) {
                return { jsonBody: null };
            }
            return { status: 500, body: err.message };
        }
    }
});

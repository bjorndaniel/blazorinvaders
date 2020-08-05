
const cosmos = require('@azure/cosmos');
const endpoint = process.env.COSMOS_API_URL;
const key = process.env.COSMOS_API_KEY;
const { CosmosClient } = cosmos;

const client = new CosmosClient({ endpoint, key });
const container = client.database("blazorinvaders").container("connections");
module.exports = async function (context, req, connection) {
    if (req.body.name && req.body.score && req.query.id && connection) {
        let res;
        try {
            res = await container.item(connection.id).delete();
            context.bindings.saveHighScore = JSON.stringify({
                id: 'CurrentHighScore',
                name: req.body.name,
                score: req.body.score
            });
        } catch (err) {
            context.res = {
                status: 400,
                body: { 'result': err }
            };
        }
        context.res = {
            // status: 200, /* Defaults to 200 */
            body: "Saved high score"
        };
    }
    else {
        context.res = {
            status: 401
        }
    }

}
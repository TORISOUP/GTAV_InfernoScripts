module.exports = {
    name: 'インフェルノMODいそのプラグイン', // @required
    uid: 'net.torisoup', // @required
    version: '0.0.1', // @required
    author: 'TORISOUP', // @required
    url: 'https://github.com/TORISOUP/GTAV_InfernoScripts', // @optional
    permissions: ['comments'], // @required
    subscribe(type, ...args) {
        for (const x of args[0].comments) {

            const data = {Command: x.data.comment.toString()};

            fetch('http://127.0.0.1:11211/', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(data) 
            })
        }
    }
}
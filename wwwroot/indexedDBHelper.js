window.indexedDBHelper = window.indexedDB || window.mozIndexedDB || window.webkitIndexedDB || window.msIndexedDB;

window.indexedDBHelper = {
    initializeDatabase: function () {
        // Inicializa o banco de dados IndexedDB com as versões e os stores necessários
        let request = indexedDB.open("BemEstarDB", 1);

        request.onerror = function (event) {
            console.error("Erro ao abrir IndexedDB", event);
        };

        request.onsuccess = function (event) {
            console.log("Banco de dados aberto com sucesso");
        };

        request.onupgradeneeded = function (event) {
            let db = event.target.result;

            if (!db.objectStoreNames.contains("usuarios")) {
                db.createObjectStore("usuarios", { keyPath: "Refeicao" });
            }

            if (!db.objectStoreNames.contains("configs")) {
                db.createObjectStore("configs", { keyPath: "IdIdentificao" });
            }
        };
    },

    saveItem: function (storeName, key, value) {
        let request = indexedDB.open("BemEstarDB", 1);

        request.onerror = function (event) {
            console.error("Erro ao abrir IndexedDB para salvar item", event);
        };

        request.onsuccess = function (event) {
            let db = event.target.result;
            let transaction = db.transaction([storeName], "readwrite");
            let store = transaction.objectStore(storeName);

            // Determinar keyPath correto para cada store
            let keyPath;
            if (storeName === "usuarios") {
                keyPath = "Refeicao";
            } else if (storeName === "configs") {
                keyPath = "IdIdentificao";
            } else {
                keyPath = "key"; // Se for uma store desconhecida
            }

            // Verificar se o objeto possui a chave correta
            if (!key) {
                console.error(`Erro: O item precisa de uma chave válida (${keyPath})`);
                return;
            }

            let item = { ...value, [keyPath]: key };

            let putRequest = store.put(item);

            putRequest.onerror = function (event) {
                console.error("Erro ao salvar item no store", event);
            };

            putRequest.onsuccess = function () {
                console.log(`Item salvo com sucesso na store ${storeName}`);
            };
        };
    },

    getItem: function (storeName, key) {
        return new Promise((resolve, reject) => {
            let request = indexedDB.open("BemEstarDB", 1);

            request.onerror = function (event) {
                console.error("Erro ao abrir IndexedDB para recuperar item", event);
                reject(event);
            };

            request.onsuccess = function (event) {
                let db = event.target.result;
                console.log(db);
                console.log(storeName)
                let transaction = db.transaction([storeName], "readonly");
                let store = transaction.objectStore(storeName);

                let getRequest = store.get(key);

                getRequest.onerror = function (event) {
                    console.error("Erro ao buscar item no store", event);
                    reject(event);
                };

                getRequest.onsuccess = function () {
                    resolve(getRequest.result);
                };
            };
        });
    },

    getAllItems: function (storeName) {
        return new Promise((resolve, reject) => {
            let request = indexedDB.open("BemEstarDB", 1);

            request.onerror = function (event) {
                console.error("Erro ao abrir IndexedDB para recuperar todos os itens", event);
                reject(event);
            };

            request.onsuccess = function (event) {
                let db = event.target.result;
                let transaction = db.transaction([storeName], "readonly");
                let store = transaction.objectStore(storeName);

                let getRequest = store.getAll();

                getRequest.onerror = function (event) {
                    console.error("Erro ao buscar todos os itens no store", event);
                    reject(event);
                };

                getRequest.onsuccess = function () {
                    resolve(getRequest.result);
                };
            };
        });
    },

    getItemCount: function (storeName) {
        return new Promise((resolve, reject) => {
            let request = indexedDB.open("BemEstarDB", 1);

            request.onerror = function (event) {
                console.error("Erro ao abrir IndexedDB para contar itens", event);
                reject(event);
            };

            request.onsuccess = function (event) {
                let db = event.target.result;
                let transaction = db.transaction([storeName], "readonly");
                let store = transaction.objectStore(storeName);

                let countRequest = store.count();

                countRequest.onerror = function (event) {
                    console.error("Erro ao contar itens no store", event);
                    reject(event);
                };

                countRequest.onsuccess = function () {
                    resolve(countRequest.result);
                };
            };
        });
    },

    deleteItem: function (storeName, key) {
        let request = indexedDB.open("BemEstarDB", 1);

        request.onerror = function (event) {
            console.error("Erro ao abrir IndexedDB para excluir item", event);
        };

        request.onsuccess = function (event) {
            let db = event.target.result;
            let transaction = db.transaction([storeName], "readwrite");
            let store = transaction.objectStore(storeName);

            let deleteRequest = store.delete(key);

            deleteRequest.onerror = function (event) {
                console.error("Erro ao excluir item do store", event);
            };

            deleteRequest.onsuccess = function () {
                console.log("Item excluído com sucesso");
            };
        };
    }
};

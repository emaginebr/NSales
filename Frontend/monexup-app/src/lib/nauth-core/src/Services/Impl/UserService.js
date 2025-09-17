//const API_URL = "https://emagine.com.br/auth-api"; 
let _httpClient;
const UserService = {
    init: function (httpClient) {
        _httpClient = httpClient;
    },
    uploadImageUser: async (file, token) => {
        let ret = {};
        const formData = new FormData();
        formData.append('file', file, 'cropped.jpg');
        //formData.append("networkId", "0");
        const request = await _httpClient.doPostFormDataAuth('/uploadImageUser', formData, token);
        if (request.success) {
            return request.data;
        }
        else {
            ret = Object.assign(Object.assign({}, ret), { mensagem: request.messageError, sucesso: false });
        }
        return ret;
    },
    getMe: async (token) => {
        let ret = {};
        const request = await _httpClient.doGetAuth('/getMe', token);
        if (request.success) {
            return request.data;
        }
        else {
            ret = Object.assign(Object.assign({}, ret), { mensagem: request.messageError, sucesso: false });
        }
        return ret;
    },
    getUserByEmail: async (email) => {
        let ret = {};
        const request = await _httpClient.doGet('/getByEmail/' + email, {});
        if (request.success) {
            return request.data;
        }
        else {
            ret = Object.assign(Object.assign({}, ret), { mensagem: request.messageError, sucesso: false });
        }
        return ret;
    },
    getBySlug: async (slug) => {
        let ret = {};
        const request = await _httpClient.doGet('/getBySlug/' + slug, {});
        if (request.success) {
            return request.data;
        }
        else {
            ret = Object.assign(Object.assign({}, ret), { mensagem: request.messageError, sucesso: false });
        }
        return ret;
    },
    insert: async (user) => {
        let ret = {};
        const request = await _httpClient.doPost('/insert', user);
        if (request.success) {
            return request.data;
        }
        else {
            ret = Object.assign(Object.assign({}, ret), { mensagem: request.messageError, sucesso: false });
        }
        return ret;
    },
    update: async (user, token) => {
        let ret = {};
        const request = await _httpClient.doPostAuth('/update', user, token);
        if (request.success) {
            return request.data;
        }
        else {
            ret = Object.assign(Object.assign({}, ret), { mensagem: request.messageError, sucesso: false });
        }
        return ret;
    },
    loginWithEmail: async (email, password) => {
        let ret = {};
        const request = await _httpClient.doPost('/loginWithEmail', {
            email: email,
            password: password,
        });
        if (request.success) {
            return request.data;
        }
        else {
            ret = Object.assign(Object.assign({}, ret), { mensagem: request.messageError, sucesso: false });
        }
        return ret;
    },
    hasPassword: async (token) => {
        let ret = {};
        const request = await _httpClient.doGetAuth('/hasPassword', token);
        if (request.success) {
            return request.data;
        }
        else {
            ret = Object.assign(Object.assign({}, ret), { mensagem: request.messageError, sucesso: false });
        }
        return ret;
    },
    changePassword: async (oldPassword, newPassword, token) => {
        let ret = {};
        const request = await _httpClient.doPostAuth('/changePassword', {
            oldPassword: oldPassword,
            newPassword: newPassword,
        }, token);
        console.log('request: ', request);
        if (request.success) {
            return request.data;
        }
        else {
            ret = Object.assign(Object.assign({}, ret), { mensagem: request.messageError, sucesso: false });
        }
        return ret;
    },
    sendRecoveryEmail: async (email) => {
        let ret = {};
        const request = await _httpClient.doGet('/sendRecoveryMail/' + email, {});
        if (request.success) {
            return request.data;
        }
        else {
            ret = Object.assign(Object.assign({}, ret), { mensagem: request.messageError, sucesso: false });
        }
        return ret;
    },
    changePasswordUsingHash: async (recoveryHash, newPassword) => {
        let ret = {};
        const request = await _httpClient.doPost('/changePasswordUsingHash', {
            recoveryHash: recoveryHash,
            newPassword: newPassword,
        });
        if (request.success) {
            return request.data;
        }
        else {
            ret = Object.assign(Object.assign({}, ret), { mensagem: request.messageError, sucesso: false });
        }
        return ret;
    },
    list: async (take) => {
        let ret = {};
        const request = await _httpClient.doGet('/list/' + take, {});
        if (request.success) {
            return request.data;
        }
        else {
            ret = Object.assign(Object.assign({}, ret), { mensagem: request.messageError, sucesso: false });
        }
        return ret;
    }
};
export default UserService;

import StringResult from "../../DTO/Services/StringResult";
import IHttpClient from "../../Infra/Interface/IHttpClient";

export default interface IImageService {
    init: (httpClient : IHttpClient) => void;
    uploadImageUser: (file: Blob, token: string) => Promise<StringResult>;
    uploadImageNetwork: (networkId: number, file: Blob, token: string) => Promise<StringResult>;
    uploadImageProduct: (productId: number, file: Blob, token: string) => Promise<StringResult>;
}
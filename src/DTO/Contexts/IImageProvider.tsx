import ProviderResult from "./ProviderResult";
import UrlProviderResult from "./UrlProviderResult";

interface IImageProvider {
    loading: boolean;

    userImage: string;
    networkImage: string;
    productImage: string;
    
    uploadImageUser: (file: Blob) => Promise<UrlProviderResult>;
    uploadImageNetwork: (networkId: number, file: Blob) => Promise<UrlProviderResult>;
    uploadImageProduct: (productId: number, file: Blob) => Promise<UrlProviderResult>;
}

export default IImageProvider;
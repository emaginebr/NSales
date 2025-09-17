import { useState } from "react";
import ProviderResult from "../../DTO/Contexts/ProviderResult";
import IImageProvider from "../../DTO/Contexts/IImageProvider";
import ImageContext from "./ImageContext";
import ImageFactory from "../../Business/Factory/ImageFactory";
import UrlProviderResult from "../../DTO/Contexts/UrlProviderResult";

export default function ImageProvider(props: any) {

    const [loading, setLoading] = useState<boolean>(false);

    const [userImage, setUserImage] = useState<string>("");
    const [networkImage, setNetworkImage] = useState<string>("");
    const [productImage, setProductImage] = useState<string>("");

    const imageProviderValue: IImageProvider = {
        loading: loading,

        userImage: userImage,
        networkImage: networkImage,
        productImage: productImage,

        uploadImageUser: async (file: Blob) => {
            let ret: Promise<UrlProviderResult>;
            setLoading(true);
            //try {
            let brt = await ImageFactory.ImageBusiness.uploadImageUser(file);
            if (brt.sucesso) {
                setLoading(false);
                setUserImage(brt.dataResult);
                return {
                    ...ret,
                    sucesso: true,
                    url: brt.dataResult,
                    mensagemSucesso: "Profile added"
                };
            }
            else {
                setLoading(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: brt.mensagem
                };
            }
            /*
            }
            catch (err) {
                setLoadingUpdate(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: JSON.stringify(err)
                };
            }
            */
        },
        uploadImageNetwork: async (networkId: number, file: Blob) => {
            let ret: Promise<UrlProviderResult>;
            setLoading(true);
            //try {
            let brt = await ImageFactory.ImageBusiness.uploadImageNetwork(networkId, file);
            if (brt.sucesso) {
                setLoading(false);
                setNetworkImage(brt.dataResult);
                return {
                    ...ret,
                    sucesso: true,
                    url: brt.dataResult,
                    mensagemSucesso: "Profile added"
                };
            }
            else {
                setLoading(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: brt.mensagem
                };
            }
            /*
            }
            catch (err) {
                setLoadingUpdate(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: JSON.stringify(err)
                };
            }
            */
        },
        uploadImageProduct: async (productId: number, file: Blob) => {
            let ret: Promise<UrlProviderResult>;
            setLoading(true);
            //try {
            let brt = await ImageFactory.ImageBusiness.uploadImageProduct(productId, file);
            if (brt.sucesso) {
                setLoading(false);
                setProductImage(brt.dataResult);
                return {
                    ...ret,
                    sucesso: true,
                    url: brt.dataResult,
                    mensagemSucesso: "Profile added"
                };
            }
            else {
                setLoading(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: brt.mensagem
                };
            }
            /*
            }
            catch (err) {
                setLoadingUpdate(false);
                return {
                    ...ret,
                    sucesso: false,
                    mensagemErro: JSON.stringify(err)
                };
            }
            */
        }
    }

    return (
        <ImageContext.Provider value={imageProviderValue}>
            {props.children}
        </ImageContext.Provider>
    );
}
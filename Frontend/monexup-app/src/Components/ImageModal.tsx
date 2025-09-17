import { useContext, useRef, useState } from "react";
import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';
import ReactCrop, { Crop } from 'react-image-crop';
import 'react-image-crop/dist/ReactCrop.css';
import ImageFactory from "../Business/Factory/ImageFactory";
import ImageContext from "../Contexts/Image/ImageContext";

enum ImageTypeEnum {
    User = 1,
    Network = 2,
    Product = 3
}

interface IImageModalParam {
    show: boolean,
    Image: ImageTypeEnum,
    networkId?: number,
    productId?: number,
    onClose: () => void,
    onSuccess?: (url: string) => void
};

function ImageModal(param: IImageModalParam) {

    const [src, setSrc] = useState<string | null>(null);
    const [crop, setCrop] = useState<Crop>({
        unit: 'px',
        width: (param.Image == ImageTypeEnum.User) ? 100 : 160,
        height: (param.Image == ImageTypeEnum.User) ? 100 : 90,
        x: 25,
        y: 25
    });
    const [croppedBlob, setCroppedBlob] = useState<Blob | null>(null);
    const imageRef = useRef<HTMLImageElement | null>(null);

    const imageContext = useContext(ImageContext);

    const onSelectFile = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files.length > 0) {
            const reader = new FileReader();
            reader.addEventListener('load', () => setSrc(reader.result as string));
            reader.readAsDataURL(e.target.files[0]);
        }
    };

    const onImageLoaded = (e: any) => {
        imageRef.current = e.currentTarget;
    };

    const getCroppedImg = async () => {
        if (!imageRef.current || !crop.width || !crop.height) return;

        const canvas = document.createElement('canvas');
        const scaleX = imageRef.current.naturalWidth / imageRef.current.width;
        const scaleY = imageRef.current.naturalHeight / imageRef.current.height;

        canvas.width = crop.width;
        canvas.height = crop.height;

        const ctx = canvas.getContext('2d');
        if (!ctx) return;

        ctx.drawImage(
            imageRef.current,
            crop.x * scaleX,
            crop.y * scaleY,
            crop.width * scaleX,
            crop.height * scaleY,
            0,
            0,
            crop.width,
            crop.height
        );

        return new Promise<Blob>((resolve) => {
            canvas.toBlob((blob) => {
                if (blob) {
                    setCroppedBlob(blob);
                    resolve(blob);
                }
            }, 'image/jpeg');
        });
    };

    const uploadImage = async () => {
        const blob = await getCroppedImg();
        if (!blob) return;

        /*
        const formData = new FormData();
        formData.append('file', blob, 'cropped.jpg');
        formData.append("networkId", "0");
        */

        switch (param.Image) {
            case ImageTypeEnum.User:
                var ret = await imageContext.uploadImageUser(blob);
                if (ret.sucesso) {
                    if (param.onSuccess) {
                        param.onSuccess(ret.url);
                    }
                }
                else {
                    alert(ret.mensagemErro);
                    return;
                }
                break;
            case ImageTypeEnum.Network:
                var ret = await imageContext.uploadImageNetwork(param.networkId, blob);
                if (ret.sucesso) {
                    if (param.onSuccess) {
                        param.onSuccess(ret.url);
                    }
                }
                else {
                    alert(ret.mensagemErro);
                    return;
                }
                break;
            case ImageTypeEnum.Product:
                var ret = await imageContext.uploadImageProduct(param.productId, blob);
                if (ret.sucesso) {
                    if (param.onSuccess) {
                        param.onSuccess(ret.url);
                    }
                }
                else {
                    alert(ret.mensagemErro);
                    return;
                }
                break;
        }
    };


    return (
        <Modal show={param.show} size="lg" onHide={param.onClose}>
            <Modal.Header closeButton>
                <Modal.Title>Upload Image</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <div>
                    <input type="file" accept="image/*" onChange={onSelectFile} />
                    {src && (
                        <ReactCrop
                            crop={crop}
                            aspect={(param.Image == ImageTypeEnum.User) ? 1 : null}
                            onChange={(newCrop) => setCrop(newCrop)}
                        >
                            <img src={src} onLoad={onImageLoaded} />
                        </ReactCrop>
                    )}
                </div>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={param.onClose}>
                    Close
                </Button>
                <Button variant="primary"
                    disabled={imageContext.loading}
                    onClick={async (e) => {
                        e.preventDefault();
                        await uploadImage();
                        param.onClose();
                    }}>
                    {imageContext.loading ? "Saving..." : "Upload Photo"}
                </Button>
            </Modal.Footer>
        </Modal>
    );
}

export { ImageModal, ImageTypeEnum };
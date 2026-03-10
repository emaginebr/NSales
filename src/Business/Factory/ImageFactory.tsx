import ServiceFactory from '../../Services/ServiceFactory';
import ImageBusiness from '../Impl/ImageBusiness';
import IImageBusiness from '../Interfaces/IImageBusiness';

const imageService = ServiceFactory.ImageService;

const imageBusinessImpl: IImageBusiness = ImageBusiness;
imageBusinessImpl.init(imageService);

const ImageFactory = {
  ImageBusiness: imageBusinessImpl
};

export default ImageFactory;

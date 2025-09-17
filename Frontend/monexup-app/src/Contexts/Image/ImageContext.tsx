import React from 'react';
import IImageProvider from '../../DTO/Contexts/IImageProvider';

const ImageContext = React.createContext<IImageProvider>(null);

export default ImageContext;
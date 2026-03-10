import React from 'react';
import ITemplateProvider from '../../DTO/Contexts/ITemplateProvider';

const TemplateContext = React.createContext<ITemplateProvider>(null);

export default TemplateContext;
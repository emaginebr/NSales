import ServiceFactory from '../../Services/ServiceFactory';
import TemplateBusiness from '../Impl/TemplateBusiness';
import ITemplateBusiness from '../Interfaces/ITemplateBusiness';

const templateService = ServiceFactory.TemplateService;

const templateBusinessImpl: ITemplateBusiness = TemplateBusiness;
templateBusinessImpl.init(templateService);

const TemplateFactory = {
  TemplateBusiness: templateBusinessImpl
};

export default TemplateFactory;

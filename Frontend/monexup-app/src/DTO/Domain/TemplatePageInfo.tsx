import { StringDictionary } from "../../Components/StringDictionary";
import TemplatePartInfo from "./TemplatePartInfo";

export default interface TemplatePageInfo {
    pageId: number;
    templateId: number;
    slug: string;
    title: string;
    parts: TemplatePartInfo[];
    variables: StringDictionary;
}
import Row from "react-bootstrap/esm/Row";
import { StringDictionary } from "../../Components/StringDictionary";
import ProductInfo from "../../DTO/Domain/ProductInfo";
import Col from "react-bootstrap/esm/Col";

interface IProductParam {
    loading: boolean,
    product: ProductInfo,
    isEditing: boolean,
    variables: StringDictionary
};

export default function Product01Part(param: IProductParam) {
    return (
        <Row>
            <Col md={12}>
                {param.product?.imageUrl &&
                    <>
                        <Row>
                            <Col md="12">
                                <div style={{
                                    width: "100%",
                                    height: "15rem",
                                    backgroundImage: "url(" + param.product?.imageUrl + ")",
                                    backgroundPosition: "center center",
                                    backgroundRepeat: "no-repeat",
                                    backgroundSize: "cover"
                                }} />
                            </Col>
                        </Row>
                        <hr />
                    </>
                }
                <h1>{param.product?.name}</h1>
                <div dangerouslySetInnerHTML={{ __html: param.product?.description }}></div>
            </Col>
        </Row>
    );
}
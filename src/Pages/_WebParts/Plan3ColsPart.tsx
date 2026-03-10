import { useNavigate, useParams } from "react-router-dom";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import Col from "react-bootstrap/esm/Col";
import Card from "react-bootstrap/esm/Card";
import CardHeader from "react-bootstrap/esm/CardHeader";
import CardTitle from "react-bootstrap/esm/CardTitle";
import CardBody from "react-bootstrap/esm/CardBody";
import CardText from "react-bootstrap/esm/CardText";
import Button from "react-bootstrap/esm/Button";
import { showFrequencyMax, showFrequencyMin } from "../../Components/Functions";
import Skeleton from "react-loading-skeleton";
import { StringDictionary } from "../../Components/StringDictionary";
import ProductInfo from "../../DTO/Domain/ProductInfo";
import EditMode from "../../Components/EditMode";
import { useTranslation } from "react-i18next";

interface IPlan3Colsaram {
    loading: boolean,
    products?: ProductInfo[],
    isEditing: boolean,
    variables: StringDictionary
};

export default function Plan3ColsPart(param: IPlan3Colsaram) {

    let navigate = useNavigate();

    const { t } = useTranslation();

    let { networkSlug, sellerSlug } = useParams();

    return (
        <>
            <section id="plans" className="py-2">
                <Container className="pb-5">
                    <Row>
                        <Col md={12} className="text-center">
                            <div className="lc-block mb-4">
                                <h2 className="display-2 mb-0">
                                    <b>{param.loading ?
                                        <Skeleton />
                                        :
                                        <>
                                            <EditMode.Text
                                                name="PLAN_TITLE"
                                                value={param.variables["PLAN_TITLE"]}
                                                isEditing={param.isEditing}
                                            />
                                        </>
                                    }</b>
                                </h2>
                                <p>{param.loading ?
                                    <Skeleton />
                                    :
                                    <>
                                        <EditMode.Text
                                            name="PLAN_DESCRIPTION"
                                            value={param.variables["PLAN_DESCRIPTION"]}
                                            isEditing={param.isEditing}
                                        />
                                    </>
                                }</p>
                            </div>
                        </Col>
                    </Row>
                    <Row md={4} className="text-center">
                        {param.loading ?
                            <>
                                {[1, 2, 3].map((index) => {
                                    return (
                                        <Col lg={4} md={6} className="text-dark my-2">
                                            <Card>
                                                <CardHeader>
                                                    <h4 className="my-0"><Skeleton /></h4>
                                                </CardHeader>
                                                <CardBody>
                                                    <CardTitle>
                                                        <span className="display-4"><Skeleton /></span>
                                                        <span className="lead"><Skeleton /></span>
                                                    </CardTitle>
                                                    <CardText className="my-4 lc-block">
                                                        <div>
                                                            <ul className="list-unstyled">
                                                                <li><Skeleton /></li>
                                                            </ul>
                                                        </div>
                                                    </CardText>
                                                    <div className="d-grid lc-block">
                                                        <Button variant="primary" size="lg" className="btn-outline-primary" disabled>Order Now</Button>
                                                    </div>
                                                </CardBody>
                                            </Card>
                                        </Col>
                                    );
                                })}
                            </>
                            :
                            param.products?.map((product) => {
                                return (
                                    <Col lg={4} md={6} className="text-dark my-2">
                                        <Card>
                                            <CardHeader>
                                                <h4 className="my-0">{product.name}</h4>
                                            </CardHeader>
                                            <CardBody>
                                                <CardTitle>
                                                    <span className="display-4"><b>${product.price}</b></span>
                                                    <span className="lead">/{showFrequencyMin(product.frequency, t)}</span>
                                                </CardTitle>
                                                <CardText className="my-4 lc-block">
                                                    <div>
                                                        <ul className="list-unstyled">
                                                            <li>{showFrequencyMax(product.frequency, t)}</li>
                                                        </ul>
                                                    </div>
                                                </CardText>
                                                <div className="d-grid lc-block">
                                                    <Button variant="primary" size="lg" className="btn-outline-primary" onClick={(e) => {
                                                        navigate(sellerSlug ?
                                                            "/" + networkSlug + "/@/" + sellerSlug + "/" + product.slug
                                                            :
                                                            "/" + networkSlug + "/" + product.slug
                                                        );
                                                    }}>Order Now</Button>
                                                </div>
                                            </CardBody>
                                        </Card>
                                    </Col>
                                );
                            })
                        }
                    </Row>
                </Container>
            </section>
        </>
    );
}
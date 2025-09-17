import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import Col from "react-bootstrap/esm/Col";
import Card from "react-bootstrap/esm/Card";
import Pagination from 'react-bootstrap/Pagination';
import ProductListPagedInfo from "../../DTO/Domain/ProductListPagedInfo";
import { Link } from "react-router-dom";
import { showFrequencyMin } from "../../Components/Functions";
import Skeleton from "react-loading-skeleton";
import EditMode from "../../Components/EditMode";
import { StringDictionary } from "../../Components/StringDictionary";
import { useTranslation } from "react-i18next";

interface IProductListParam {
    loading: boolean;
    networkSlug: string;
    sellerSlug?: string;
    ProductResult: ProductListPagedInfo;
    isEditing: boolean,
    variables: StringDictionary,
    onChangePage: (pageNum: number) => void;
}

export default function ProductListPart(param: IProductListParam) {

    const { t } = useTranslation();

    return (

        <Container className="py-4 py-lg-6">
            <Row>
                <Col md={12} className="text-center">
                    <div className="lc-block mb-1">
                        <h2 className="display-2 mb-0">
                            <b>{param.loading ?
                                <Skeleton />
                                :
                                <>
                                    <EditMode.Text
                                        name="PRODUCT_TITLE"
                                        value={param.variables["PRODUCT_TITLE"]}
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
                                    name="PRODUCT_DESCRIPTION"
                                    value={param.variables["PRODUCT_DESCRIPTION"]}
                                    isEditing={param.isEditing}
                                />
                            </>
                        }</p>
                    </div>
                </Col>
            </Row>
            {param.loading &&
                <Row>
                    <Col lg={4}>
                        <div className="d-flex justify-content-center">
                            <div className="spinner-border" role="status">
                                <span className="visually-hidden">Loading...</span>
                            </div>
                        </div>
                    </Col>
                </Row>
            }
            <Row className="mb-5">
                {!param.loading && param.ProductResult?.products.map((product) => {
                    return (
                        <Col lg={4}>
                            <Card className="shadow-lg p-3">
                                <Card.Img src={product.imageUrl} />
                                <Card.Body>
                                    <div className="lc-block text-center mb-3">
                                        <div>
                                            <h3><Link to={"/@/" + param.sellerSlug + "/" + product.slug}>{product.name}</Link></h3>
                                        </div>
                                    </div>
                                    <div className="lc-block text-center">
                                        ${product.price} / {showFrequencyMin(product.frequency, t)}
                                    </div>
                                </Card.Body>
                            </Card>
                        </Col>
                    );
                })}
            </Row>
            {!param.loading && param.ProductResult &&
                <Row>
                    <Col md={12} className="text-center">
                        <Pagination className="justify-content-center">
                            <Pagination.First
                                disabled={!(param.ProductResult?.pageNum > 1)}
                                onClick={() => param.onChangePage(1)} />
                            <Pagination.Prev
                                disabled={!(param.ProductResult?.pageNum > 1)}
                                onClick={() => param.onChangePage(param.ProductResult?.pageNum - 1)} />
                            <Pagination.Ellipsis />

                            {param.ProductResult?.pageNum - 2 >= 1 &&
                                <Pagination.Item
                                    onClick={() => param.onChangePage(param.ProductResult?.pageNum - 2)}
                                >{param.ProductResult?.pageNum - 2}</Pagination.Item>
                            }
                            {param.ProductResult?.pageNum - 1 >= 1 &&
                                <Pagination.Item
                                    onClick={() => param.onChangePage(param.ProductResult?.pageNum - 1)}
                                >{param.ProductResult?.pageNum - 1}</Pagination.Item>
                            }
                            <Pagination.Item active>{param.ProductResult?.pageNum}</Pagination.Item>
                            {param.ProductResult?.pageNum + 1 <= param.ProductResult?.pageCount &&
                                <Pagination.Item
                                    onClick={() => param.onChangePage(param.ProductResult?.pageNum + 1)}
                                >{param.ProductResult?.pageNum + 1}</Pagination.Item>
                            }
                            {param.ProductResult?.pageNum + 2 <= param.ProductResult?.pageCount &&
                                <Pagination.Item
                                    onClick={() => param.onChangePage(param.ProductResult?.pageNum + 2)}
                                >{param.ProductResult?.pageNum + 2}</Pagination.Item>
                            }

                            <Pagination.Ellipsis />
                            <Pagination.Next
                                disabled={!(param.ProductResult?.pageNum < param.ProductResult?.pageCount)}
                                onClick={() => param.onChangePage(param.ProductResult?.pageCount)}
                            />
                            <Pagination.Last
                                disabled={!(param.ProductResult?.pageNum < param.ProductResult?.pageCount)}
                                onClick={() => param.onChangePage(param.ProductResult?.pageCount)} />
                        </Pagination>
                    </Col>
                </Row>
            }
        </Container>
    );
}


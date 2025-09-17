import { useContext, useEffect, useState } from "react";
import Col from "react-bootstrap/esm/Col";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import Form from 'react-bootstrap/Form';
import AuthContext from "../../Contexts/Auth/AuthContext";
import Button from "react-bootstrap/esm/Button";
import Card from 'react-bootstrap/Card';
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faAddressBook, faArrowLeft, faArrowRight, faBitcoinSign, faCalendar, faCalendarAlt, faCancel, faClose, faCode, faDollar, faEnvelope, faEthernet, faIdCard, faLock, faPercent, faPhone, faSave, faSignInAlt, faTrash, faUpload, faUser } from '@fortawesome/free-solid-svg-icons';
import Table from "react-bootstrap/esm/Table";
import { Link, useNavigate, useParams } from "react-router-dom";
import InputGroup from 'react-bootstrap/InputGroup';
import UserContext from "../../Contexts/User/UserContext";
import MessageToast from "../../Components/MessageToast";
import Moment from 'moment';
import { MessageToastEnum } from "../../DTO/Enum/MessageToastEnum";
import { CustomToolbar } from "../../Components/CustomToolbar";
import ReactQuill from "react-quill";
import NetworkContext from "../../Contexts/Network/NetworkContext";
import ProductContext from "../../Contexts/Product/ProductContext";
import ProductInfo from "../../DTO/Domain/ProductInfo";
import { ProductStatusEnum } from "../../DTO/Enum/ProductStatusEnum";
import 'react-quill/dist/quill.snow.css';
import { ImageModal, ImageTypeEnum } from "../../Components/ImageModal";
import ImageContext from "../../Contexts/Image/ImageContext";
import { useTranslation } from "react-i18next";

export default function ProductEditPage() {

    const { t } = useTranslation();


    const networkContext = useContext(NetworkContext);
    const productContext = useContext(ProductContext);

    let { productId } = useParams();

    const [insertMode, setInsertMode] = useState<boolean>(false);
    const [showImageModal, setShowImageModal] = useState<boolean>(false);

    const [dialog, setDialog] = useState<MessageToastEnum>(MessageToastEnum.Error);
    const [showMessage, setShowMessage] = useState<boolean>(false);
    const [messageText, setMessageText] = useState<string>("");

    let navigate = useNavigate();
    Moment.locale('en');

    const throwError = (message: string) => {
        setDialog(MessageToastEnum.Error)
        setMessageText(message);
        setShowMessage(true);
    };
    const showSuccessMessage = (message: string) => {
        setDialog(MessageToastEnum.Success)
        setMessageText(message);
        setShowMessage(true);
    };

    useEffect(() => {
        let productIdNum: number = parseInt(productId);
        if (productIdNum > 0) {
            productContext.getById(productIdNum).then((ret) => {
                if (!ret.sucesso) {
                    throwError(ret.mensagemErro);
                    return;
                }
                setInsertMode(false);
            });
            return;
        }
        setInsertMode(true);
        let product: ProductInfo = null;
        productContext.setProduct({
            ...product,
            networkId: networkContext.userNetwork?.networkId,
            status: ProductStatusEnum.Active
        });
    }, []);

    return (
        <>
            <MessageToast
                dialog={dialog}
                showMessage={showMessage}
                messageText={messageText}
                onClose={() => setShowMessage(false)}
            ></MessageToast>
            <ImageModal
                Image={ImageTypeEnum.Product}
                productId={productContext.product?.productId}
                show={showImageModal}
                onClose={() => setShowImageModal(false)}
                onSuccess={(url: string) => {
                    //navigate("/admin/products/" + productId);
                    productContext.setProduct({
                        ...productContext.product,
                        imageUrl: url
                    });
                }}
            />
            <Container>
                <Row>
                    <Col md={12}>
                        <h3>
                            <nav aria-label="breadcrumb">
                                <ol className="breadcrumb">
                                    <li className="breadcrumb-item"><Link to="/admin/dashboard">{t('breadcrumb_my_network')}</Link></li>
                                    <li className="breadcrumb-item"><Link to="/admin/products">{t('breadcrumb_products_list')}</Link></li>
                                    <li className="breadcrumb-item active" aria-current="page">{t('breadcrumb_my_product')}</li>
                                </ol>
                            </nav>
                        </h3>
                    </Col>
                </Row>
                <Card>
                    <Card.Body>
                        <Row>
                            <Col md="4" className="text-center">
                                <div className="mb-2">
                                    {productContext.product?.imageUrl &&
                                        <img src={productContext.product?.imageUrl} style={{ width: "100%", height: "auto" }} />
                                    }
                                </div>
                                <div className="lc-block d-grid gap-3 d-md-block">
                                    {productContext.product?.productId > 0 &&
                                        <Button variant="primary" className="me-md-2" size="lg" onClick={async (e) => {
                                            e.preventDefault();
                                            setShowImageModal(true);
                                        }}>
                                            <FontAwesomeIcon icon={faUpload} fixedWidth />&nbsp;{t('product_edit_change_image_button')}
                                        </Button>
                                    }
                                </div>
                            </Col>
                            <Col md="8">
                                <Form>
                                    <div className="text-center mb-3"> {/* TODO: Confirm if this text is appropriate here or should be changed/removed */}
                                        {t('network_edit_registration_info')}
                                    </div>
                                    <Form.Group as={Row} className="mb-3">
                                        <Form.Label column sm="2">{t('form_label_name')}:</Form.Label>
                                        <Col sm="10">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faUser} fixedWidth /></InputGroup.Text>
                                                <Form.Control type="text" size="lg"
                                                    placeholder={t('product_edit_name_placeholder')}
                                                    value={productContext.product?.name}
                                                    onChange={(e) => {
                                                        productContext.setProduct({
                                                            ...productContext.product,
                                                            name: e.target.value
                                                        });
                                                    }} />
                                            </InputGroup>
                                        </Col>
                                    </Form.Group>
                                    {productContext.product?.productId > 0 &&
                                        <Form.Group as={Row} className="mb-3">
                                            <Form.Label column sm="2">{t('form_label_slug')}:</Form.Label>
                                            <Col sm="10">
                                                <InputGroup>
                                                    <InputGroup.Text><FontAwesomeIcon icon={faCode} fixedWidth /></InputGroup.Text>
                                                    <Form.Control type="text" size="lg"
                                                        placeholder={t('product_edit_slug_placeholder')}
                                                        value={productContext.product?.slug}
                                                        onChange={(e) => {
                                                            productContext.setProduct({
                                                                ...productContext.product,
                                                                slug: e.target.value
                                                            });
                                                        }} />
                                                </InputGroup>
                                            </Col>
                                        </Form.Group>
                                    }
                                    <Form.Group as={Row} className="mb-3">
                                        <Form.Label column sm="2">{t('product_edit_frequency_label')}:</Form.Label>
                                        <Col sm="5">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faCalendar} fixedWidth /></InputGroup.Text>
                                                <Form.Select size="lg"
                                                    value={productContext.product?.frequency}
                                                    onChange={(e) => {
                                                        //alert(e.target.value);
                                                        productContext.setProduct({
                                                            ...productContext.product,
                                                            frequency: parseInt(e.target.value)
                                                        });
                                                    }}
                                                >
                                                    <option value={0}>{t('product_edit_frequency_one_time')}</option>
                                                    <option value={30}>{t('product_edit_frequency_monthly')}</option>
                                                    <option value={365}>{t('product_edit_frequency_annually')}</option>
                                                </Form.Select>
                                            </InputGroup>
                                        </Col>
                                        <Form.Label column sm="1">{t('product_edit_price_label')}:</Form.Label>
                                        <Col sm="4">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faDollar} fixedWidth /></InputGroup.Text>
                                                <Form.Control type="number" size="lg"
                                                    placeholder={t('product_edit_price_placeholder')}
                                                    value={productContext.product?.price}
                                                    onChange={(e) => {
                                                        productContext.setProduct({
                                                            ...productContext.product,
                                                            price: parseFloat(e.target.value)
                                                        });
                                                    }} />
                                            </InputGroup>
                                        </Col>
                                    </Form.Group>
                                    <Form.Group as={Row} className="mb-3">
                                        <Form.Label column sm="2">{t('product_edit_status_label')}:</Form.Label>
                                        <Col sm="10">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faPercent} fixedWidth /></InputGroup.Text>
                                                <Form.Select size="lg"
                                                    value={productContext.product?.status}
                                                    onChange={(e) => {
                                                        productContext.setProduct({
                                                            ...productContext.product,
                                                            status: parseInt(e.target.value)
                                                        });
                                                    }} >
                                                    <option value={ProductStatusEnum.Active}>{t('product_status_active')}</option>
                                                    <option value={ProductStatusEnum.Inactive}>{t('product_status_inactive')}</option>
                                                    <option value={ProductStatusEnum.Expired}>{t('product_status_expired')}</option>
                                                </Form.Select>
                                            </InputGroup>
                                        </Col>
                                    </Form.Group>
                                    <div className="py-3">
                                        <CustomToolbar />
                                        <ReactQuill
                                            theme="snow"
                                            value={productContext.product?.description}
                                            placeholder={t('product_edit_description_placeholder')}
                                            onChange={(value) => {
                                                productContext.setProduct({
                                                    ...productContext.product,
                                                    description: value
                                                });
                                            }}
                                            modules={{
                                                toolbar: {
                                                    container: "#custom-toolbar",
                                                },
                                            }}
                                            formats={[
                                                "header",
                                                "bold",
                                                "italic",
                                                "underline",
                                                "size",
                                                "link",
                                                "clean",
                                            ]}
                                        />
                                    </div>
                                    <div className="d-grid gap-2 d-md-flex justify-content-md-end">
                                        <Button variant="danger" size="lg" onClick={() => {
                                            navigate("/admin/products"); // TODO: Consider if this route should be dynamic or translated if it's a label
                                        }}><FontAwesomeIcon icon={faArrowLeft} fixedWidth /> {t('back_button')}</Button>
                                        <Button variant="success" size="lg" onClick={async (e) => {
                                            if (insertMode) {
                                                productContext.setProduct({
                                                    ...productContext.product,
                                                    productId: 0,
                                                    networkId: networkContext.userNetwork?.networkId
                                                });
                                                let ret = await productContext.insert(productContext.product);
                                                if (ret.sucesso) {
                                                    showSuccessMessage(ret.mensagemSucesso ? ret.mensagemSucesso : t('product_edit_insert_success_message'));
                                                    //alert(userContext.user?.id);
                                                }
                                                else {
                                                    throwError(ret.mensagemErro);
                                                }
                                            }
                                            else {
                                                let ret = await productContext.update(productContext.product);
                                                if (ret.sucesso) {
                                                    //alert(userContext.user?.id);
                                                    showSuccessMessage(ret.mensagemSucesso ? ret.mensagemSucesso : t('product_edit_update_success_message'));
                                                }
                                                else {
                                                    throwError(ret.mensagemErro);
                                                }
                                            }
                                        }}
                                            disabled={productContext.loadingUpdate}
                                        >
                                            {productContext.loadingUpdate ? t('loading') :
                                                <>
                                                    <FontAwesomeIcon icon={faSave} fixedWidth />&nbsp;{t('save_button')}
                                                </>}
                                        </Button>
                                    </div>
                                </Form>
                            </Col>
                        </Row>
                    </Card.Body>
                </Card>
            </Container>
        </>
    );
}
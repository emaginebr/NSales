import Nav from 'react-bootstrap/Nav';
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faArrowDown, faArrowUp, faEdit, faImagePortrait, faPlus, faTrash, faUpload } from "@fortawesome/free-solid-svg-icons";
import { ReactNode, useContext, useState } from 'react';
import Button, { ButtonProps } from 'react-bootstrap/esm/Button';
import Modal from 'react-bootstrap/Modal';
import Form from 'react-bootstrap/Form';
import InputGroup from 'react-bootstrap/InputGroup';
import { getTemplatePartTitle, partEnumToStr, WebsitePartEnum } from './TemplateUtils';
import TemplatePartInfo from '../DTO/Domain/TemplatePartInfo';
import TemplateContext from '../Contexts/Template/TemplateContext';
import { useParams } from 'react-router-dom';
import AuthContext from '../Contexts/Auth/AuthContext';
import TemplateVarInfo from '../DTO/Domain/TemplateVarInfo';

interface IEditModeProps {
    children: ReactNode;
    isEditing?: boolean;
    part: TemplatePartInfo;
    acceptableParts: WebsitePartEnum[];
};

interface IEditModeNewProps {
    isEditing?: boolean;
    acceptableParts: WebsitePartEnum[];
};

interface IEditModeTextProps {
    name: string;
    value: string;
    isEditing?: boolean;
};

interface IEditModeBtnProps extends ButtonProps {
    name: string;
    value: string;
    isEditing?: boolean;
};

interface IEditModeImgProps extends React.ButtonHTMLAttributes<HTMLImageElement> {
    name: string;
    defaultSrc: string;
    isEditing?: boolean;
};

interface IEditModeModalProps {
    show: boolean,
    loading?: boolean,
    onSave: () => void;
    onClose: () => void
};

interface IEditModeModalVarProps extends IEditModeModalProps {
    vars?: TemplateVarInfo;
    setVars: (value: TemplateVarInfo) => void;
};

interface IEditModeModalPartProps extends IEditModeModalProps {
    part?: TemplatePartInfo;
    setPart: (value: TemplatePartInfo) => void;
    acceptableParts: WebsitePartEnum[];
};

const New: React.FC<IEditModeNewProps> = ({ acceptableParts, isEditing = true }) => {

    const [showModal, setShowModal] = useState<boolean>(false);

    const authContext = useContext(AuthContext);
    const templateContext = useContext(TemplateContext);

    if (!isEditing) {
        return <></>;
    }
    return (
        <>
            <EditModeModal
                show={showModal}
                loading={templateContext.loadingUpdate}
                onSave={async () => {
                    let ret = await templateContext.insertPart(templateContext.part);
                    if (!ret.sucesso) {
                        alert(ret.mensagemErro);
                        return;
                    }
                    let retLd = await templateContext.getPageById(templateContext.page?.pageId, authContext.language);
                    if (!retLd.sucesso) {
                        alert(retLd.mensagemErro);
                        return;
                    }
                    setShowModal(false);
                }}
                part={templateContext.part}
                setPart={(part) => templateContext.setPart(part)}
                onClose={() => setShowModal(false)}
                acceptableParts={acceptableParts}
            />
            <section className="editmode-new text-center">
                <a href="#" onClick={(e) => {
                    e.preventDefault();
                    let part: TemplatePartInfo;
                    templateContext.setPart({
                        ...part,
                        pageId: templateContext.page?.pageId,
                        partKey: partEnumToStr(acceptableParts[0])
                    });
                    setShowModal(true);
                }}><FontAwesomeIcon icon={faPlus} fixedWidth /> Click here to create a new website part</a>
            </section>
        </>
    )
};

const Text: React.FC<IEditModeTextProps> = ({ name, value, isEditing = true }) => {

    const [showModal, setShowModal] = useState<boolean>(false);

    const authContext = useContext(AuthContext);
    const templateContext = useContext(TemplateContext);

    const reloadPage = async () => {
        let ret = await templateContext.getPageById(templateContext.page?.pageId, authContext.language);
        if (!ret.sucesso) {
            alert(ret.mensagemErro);
            return;
        }
    };

    if (!isEditing) {
        return <>{value ?? name}</>;
    }
    return (
        <>
            <EditModeTextModal
                show={showModal}
                loading={templateContext.loadingUpdate}
                vars={templateContext.variable}
                setVars={(vars) => templateContext.setVariable(vars)}
                onSave={ async () => { 
                    let ret = await templateContext.saveVariable(templateContext.variable);
                    if (!ret.sucesso) {
                        alert(ret.mensagemErro);
                        return;
                    }
                    await reloadPage();
                    setShowModal(false);
                }}
                onClose={() => setShowModal(false)}
            />
            <span className="editmode-text">{value ?? name}
                <a href="#" onClick={ async (e) => {
                    e.preventDefault();
                    let ret = await templateContext.getVariable(templateContext.page?.pageId, name);
                    if (!ret.sucesso) {
                        alert(ret.mensagemErro);
                        return;
                    }
                    setShowModal(true);
                }}><FontAwesomeIcon icon={faEdit} fixedWidth /></a></span>
        </>
    )
};

const Btn: React.FC<IEditModeBtnProps> = ({ name, value, isEditing = true, ...rest }) => {

    const [showModal, setShowModal] = useState<boolean>(false);

    const authContext = useContext(AuthContext);
    const templateContext = useContext(TemplateContext);

    const reloadPage = async () => {
        let ret = await templateContext.getPageById(templateContext.page?.pageId, authContext.language);
        if (!ret.sucesso) {
            alert(ret.mensagemErro);
            return;
        }
    };

    if (!isEditing) {
        return (
            <Button {...rest}>{value ?? name}</Button>
        );
    }
    return (
        <>
            <EditModeTextModal
                show={showModal}
                loading={templateContext.loadingUpdate}
                vars={templateContext.variable}
                setVars={(vars) => templateContext.setVariable(vars)}
                onSave={ async () => { 
                    let ret = await templateContext.saveVariable(templateContext.variable);
                    if (!ret.sucesso) {
                        alert(ret.mensagemErro);
                        return;
                    }
                    await reloadPage();
                    setShowModal(false);
                }}
                onClose={() => setShowModal(false)}
            />
            <div className="editmode-text">
                <Button {...rest}>{value ?? name}</Button>
                <a href="#" onClick={ async (e) => {
                    e.preventDefault();
                    let ret = await templateContext.getVariable(templateContext.page?.pageId, name);
                    if (!ret.sucesso) {
                        alert(ret.mensagemErro);
                        return;
                    }
                    setShowModal(true);
                }}><FontAwesomeIcon icon={faEdit} fixedWidth /></a>
            </div>
        </>
    )
};

const Img: React.FC<IEditModeImgProps> = ({ name, defaultSrc, isEditing = true, ...rest }) => {

    const [showModal, setShowModal] = useState<boolean>(false);

    if (!isEditing) {
        return (
            <img src={defaultSrc} {...rest} />
        );
    }
    return (
        <>
            <EditModeUploadModal
                show={showModal}
                onSave={() => { }}
                onClose={() => setShowModal(false)}
            />
            <div className="editmode-img">
                <Button variant='success'
                    className="editmode-upload-btn"
                    onClick={(e) => {
                        e.preventDefault();
                        setShowModal(true);
                    }}>
                    <FontAwesomeIcon icon={faUpload} fixedWidth />
                </Button>
                <img src={defaultSrc} {...rest} />
            </div>
        </>
    )
};

const EditModeModal = (param: IEditModeModalPartProps) => {

    return (
        <Modal show={param.show} onHide={() => param.onClose()}>
            <Modal.Header closeButton>
                <Modal.Title>Edit Web Part</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Form>
                    <Form.Group className="mb-3">
                        <Form.Label>Website Part:</Form.Label>
                        <InputGroup>
                            <InputGroup.Text>
                                <FontAwesomeIcon icon={faImagePortrait} fixedWidth />
                            </InputGroup.Text>
                            <Form.Select size="lg"
                                value={param.part?.partKey}
                                onChange={(e) => {
                                    param.setPart({
                                        ...param.part,
                                        partKey: e.target.value
                                    });
                                }}>
                                {param.acceptableParts.map((part) => {
                                    return (
                                        <option value={partEnumToStr(part)}>
                                            {getTemplatePartTitle(part)}
                                        </option>
                                    );
                                })}
                            </Form.Select>
                        </InputGroup>
                    </Form.Group>
                </Form>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={() => param.onClose()}>
                    Close
                </Button>
                <Button variant="primary" disabled={param.loading}
                    onClick={async (e) => {
                        e.preventDefault();
                        param.onSave();
                    }}>
                    {param.loading ? "Saving..." : "Save"}
                </Button>
            </Modal.Footer>
        </Modal>
    );
};

const EditModeTextModal = (param: IEditModeModalVarProps) => {

    return (
        <Modal show={param.show} onHide={() => param.onClose()}>
            <Modal.Header closeButton>
                <Modal.Title>Change Text</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Form>
                    <Form.Group className="mb-3">
                        <InputGroup>
                            <InputGroup.Text>
                                <img src={process.env.PUBLIC_URL + "/flags/gb.svg"} style={{ width: "21px", height: "21px" }} />
                            </InputGroup.Text>
                            <Form.Control type="text" 
                                placeholder="Inglês" 
                                value={param.vars?.english} 
                                onChange={(e) => {
                                    param.setVars({
                                        ...param.vars,
                                        english: e.target.value
                                    });
                                }} />
                        </InputGroup>
                    </Form.Group>
                    <Form.Group className="mb-3">
                        <InputGroup>
                            <InputGroup.Text>
                                <img src={process.env.PUBLIC_URL + "/flags/fr.svg"} style={{ width: "21px", height: "21px" }} />
                            </InputGroup.Text>
                            <Form.Control type="text"
                                placeholder="Francês" 
                                value={param.vars?.french} 
                                onChange={(e) => {
                                    param.setVars({
                                        ...param.vars,
                                        french: e.target.value
                                    });
                                }} />
                        </InputGroup>
                    </Form.Group>
                    <Form.Group className="mb-3">
                        <InputGroup>
                            <InputGroup.Text>
                                <img src={process.env.PUBLIC_URL + "/flags/es.svg"} style={{ width: "21px", height: "21px" }} />
                            </InputGroup.Text>
                            <Form.Control type="text"
                                placeholder="Espanhol" 
                                value={param.vars?.spanish} 
                                onChange={(e) => {
                                    param.setVars({
                                        ...param.vars,
                                        spanish: e.target.value
                                    });
                                }} />
                        </InputGroup>
                    </Form.Group>
                    <Form.Group className="mb-3">
                        <InputGroup>
                            <InputGroup.Text>
                                <img src={process.env.PUBLIC_URL + "/flags/br.svg"} style={{ width: "21px", height: "21px" }} />
                            </InputGroup.Text>
                            <Form.Control type="text"
                                placeholder="Português" 
                                value={param.vars?.portuguese} 
                                onChange={(e) => {
                                    param.setVars({
                                        ...param.vars,
                                        portuguese: e.target.value
                                    });
                                }} />
                        </InputGroup>
                    </Form.Group>
                </Form>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={() => param.onClose()}>
                    Close
                </Button>
                <Button variant="primary" disabled={param.loading}
                    onClick={async (e) => {
                        e.preventDefault();
                        param.onSave();
                    }}>
                    {param.loading ? "Saving..." : "Save"}
                </Button>
            </Modal.Footer>
        </Modal>
    );
};

const EditModeUploadModal = (param: IEditModeModalProps) => {

    return (
        <Modal show={param.show} onHide={() => param.onClose()}>
            <Modal.Header closeButton>
                <Modal.Title>Upload Image</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Form>
                    <Form.Group className="mb-3">
                        <Form.Control type="file" />
                    </Form.Group>
                </Form>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={() => param.onClose()}>
                    Close
                </Button>
                <Button variant="primary"
                    onClick={async (e) => {
                        e.preventDefault();
                    }}>
                    Upload
                </Button>
            </Modal.Footer>
        </Modal>
    );
};


const EditMode: React.FC<IEditModeProps> & {
    New: React.FC<IEditModeNewProps>,
    Text: React.FC<IEditModeTextProps>,
    Btn: React.FC<IEditModeBtnProps>,
    Img: React.FC<IEditModeImgProps>
} = ({ children = "", part = null, acceptableParts, isEditing = true }) => {

    const [showModal, setShowModal] = useState<boolean>(false);

    const authContext = useContext(AuthContext);
    const templateContext = useContext(TemplateContext);

    const reloadPage = async () => {
        let ret = await templateContext.getPageById(templateContext.page?.pageId, authContext.language);
        if (!ret.sucesso) {
            alert(ret.mensagemErro);
            return;
        }
    };

    if (!isEditing) {
        return <>{children}</>;
    }

    return (
        <>
            <EditModeModal
                show={showModal}
                loading={templateContext.loadingUpdate}
                onSave={async () => {
                    let ret = await templateContext.updatePart(templateContext.part);
                    if (!ret.sucesso) {
                        alert(ret.mensagemErro);
                        return;
                    }
                    await reloadPage();
                    setShowModal(false);
                }}
                onClose={() => setShowModal(false)}
                part={templateContext.part}
                setPart={(part) => templateContext.setPart(part)}
                acceptableParts={acceptableParts}
            />
            <section className="editmode">
                <div className="flex-column editmode-bar">
                    <div className="lc-block text-center mb-1">
                        <Button variant="primary" onClick={async (e) => {
                            e.preventDefault();
                            if (part) {
                                let ret = await templateContext.movePartUp(part.partId);
                                if (!ret.sucesso) {
                                    alert(ret.mensagemErro);
                                    return;
                                }
                                await reloadPage();
                            }
                        }}>
                            <FontAwesomeIcon icon={faArrowUp} fixedWidth />
                        </Button>
                    </div>
                    <div className="lc-block text-center mb-1">
                        <Button variant="success" onClick={(e) => {
                            e.preventDefault();
                            templateContext.setPart(part);
                            setShowModal(true);
                        }}>
                            <FontAwesomeIcon icon={faEdit} fixedWidth />
                        </Button>
                    </div>
                    <div className="lc-block text-center mb-1">
                        <Button variant="danger" onClick={async (e) => {
                            e.preventDefault();
                            if (part) {
                                let ret = await templateContext.deletePart(part.partId);
                                if (!ret.sucesso) {
                                    alert(ret.mensagemErro);
                                    return;
                                }
                                await reloadPage();
                            }
                        }}>
                            <FontAwesomeIcon icon={faTrash} fixedWidth />
                        </Button>
                    </div>
                    <div className="lc-block text-center mb-1">
                        <Button variant="primary" onClick={async (e) => {
                            e.preventDefault();
                            if (part) {
                                let ret = await templateContext.movePartDown(part.partId);
                                if (!ret.sucesso) {
                                    alert(ret.mensagemErro);
                                    return;
                                }
                                await reloadPage();
                            }
                        }}>
                            <FontAwesomeIcon icon={faArrowDown} fixedWidth />
                        </Button>
                    </div>
                </div>
                {children}
            </section>
        </>
    );
}

EditMode.New = New;
EditMode.Text = Text;
EditMode.Btn = Btn;
EditMode.Img = Img;

export default EditMode;
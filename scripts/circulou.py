import os
import sys
import re
import requests
from dotenv import load_dotenv
from openai import OpenAI

load_dotenv(override=True)

NAUTH_EMAIL = os.getenv("NAUTH_EMAIL")
NAUTH_PASSWORD = os.getenv("NAUTH_PASSWORD")
NAUTH_URL = os.getenv("NAUTH_URL")
LOFN_URL = os.getenv("LOFN_URL")
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")

TENANT_ID = "emagine"
DEVICE_FINGERPRINT = "seed-script"
USER_AGENT = "LofnSeedScript/1.0"

COMMON_HEADERS = {
    "X-Tenant-Id": TENANT_ID,
    "X-Device-Fingerprint": DEVICE_FINGERPRINT,
    "User-Agent": USER_AGENT,
}

PHOTOS_DIR = os.path.join(os.path.dirname(__file__), "photos", "circulou")
os.makedirs(PHOTOS_DIR, exist_ok=True)

ROOT_CATEGORY = "Roupas"

CATEGORIES = {
    "Camisetas": [
        {
            "name": "Camiseta Vintage Rock 80s",
            "price": 49.90,
            "discount": 0,
            "featured": True,
            "description": (
                "## Camiseta Vintage Rock 80s\n\n"
                "Peça **autêntica dos anos 80** garimpada em coleção particular. "
                "Estampa original de turnê, algodão envelhecido com toque macio.\n\n"
                "### Detalhes da Peça\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Tamanho | M (medidas: 52cm peito x 68cm comprimento) |\n"
                "| Composição | 100% algodão |\n"
                "| Conservação | Muito boa — leve desbote característico |\n"
                "| Década | 1980 |\n"
                "| Origem | EUA (importada) |\n\n"
                "### Destaques\n\n"
                "- Peça **autêntica** com etiqueta original Hanes USA\n"
                "- Estampa **screen print** original — sem reprodução\n"
                "- **Desbote natural** que só o tempo faz\n"
                "- Caimento **oversized** clássico da época\n\n"
                "> *Cada camiseta vintage tem uma história — agora ela pode ser sua.*"
            ),
        },
        {
            "name": "Camiseta Básica Algodão Pima",
            "price": 29.90,
            "discount": 10,
            "featured": False,
            "description": (
                "## Camiseta Básica Algodão Pima\n\n"
                "Camiseta básica em **algodão pima peruano** — extremamente macia, "
                "respirável e durável. Pouquíssimo uso, como nova.\n\n"
                "### Detalhes da Peça\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Tamanho | G (medidas: 56cm peito x 72cm comprimento) |\n"
                "| Composição | 100% algodão pima |\n"
                "| Cor | Off-white |\n"
                "| Conservação | Excelente — usada poucas vezes |\n"
                "| Marca | Reserva |\n\n"
                "### Destaques\n\n"
                "- **Algodão pima** — fibra longa, mais macio que algodão comum\n"
                "- Costura **reforçada** em ombros e gola\n"
                "- Caimento **regular fit** versátil\n"
                "- Peça **circular**: rodou pouco, ainda tem muito quilômetro pela frente\n\n"
                "> *Básico de qualidade não passa de moda — circulou para você prolongar a vida dela.*"
            ),
        },
        {
            "name": "Camiseta Tie Dye Anos 90",
            "price": 39.90,
            "discount": 0,
            "featured": False,
            "description": (
                "## Camiseta Tie Dye Anos 90\n\n"
                "Tie dye **autêntico dos anos 90** com pigmentos vibrantes que "
                "resistiram ao tempo. Espiral clássica em tons de azul e roxo.\n\n"
                "### Detalhes da Peça\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Tamanho | P (medidas: 48cm peito x 65cm comprimento) |\n"
                "| Composição | 100% algodão |\n"
                "| Padrão | Espiral azul/roxo/branco |\n"
                "| Conservação | Boa — desbote leve nas dobras |\n"
                "| Década | 1990 |\n\n"
                "### Destaques\n\n"
                "- **Tie dye original** feito à mão (sem reprodução digital)\n"
                "- Pigmentos **fixados em fibra natural**\n"
                "- Peça **única** — nenhuma camiseta tie dye é igual\n"
                "- Caimento **slim** típico dos anos 90\n\n"
                "> *Cor que conta história — peça com personalidade impossível de imitar.*"
            ),
        },
    ],
    "Calças": [
        {
            "name": "Calça Jeans Levi's 501 Vintage",
            "price": 149.90,
            "discount": 15,
            "featured": True,
            "description": (
                "## Calça Jeans Levi's 501 Vintage\n\n"
                "**Levi's 501** original — o jeans mais icônico de todos os tempos. "
                "Lavagem desbotada natural, costuras impecáveis, fivela de cobre original.\n\n"
                "### Detalhes da Peça\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Tamanho | 38 (cintura: 78cm, comprimento: 102cm) |\n"
                "| Composição | 100% algodão denim |\n"
                "| Modelo | 501 Original Fit |\n"
                "| Conservação | Boa — desbote uniforme natural |\n"
                "| Origem | Made in USA (etiqueta vermelha) |\n\n"
                "### Destaques\n\n"
                "- **Levi's 501** com etiqueta vermelha **Made in USA**\n"
                "- Fechamento de **botões** (cinco botões originais Levi's)\n"
                "- **Selvedge denim** com barra original\n"
                "- Desbote **natural** que vale mais que jeans novo\n"
                "- Caimento **straight leg** atemporal\n\n"
                "> *Não existe jeans melhor que um Levi's 501 com história — é peça de coleção.*"
            ),
        },
        {
            "name": "Calça Wide Leg Linho",
            "price": 89.90,
            "discount": 0,
            "featured": False,
            "description": (
                "## Calça Wide Leg Linho\n\n"
                "Calça **wide leg em linho puro** — perua, fresca e elegante. "
                "Cor caramelo natural, perfeita para verão e looks despojados.\n\n"
                "### Detalhes da Peça\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Tamanho | M (cintura: 76cm, comprimento: 104cm) |\n"
                "| Composição | 100% linho |\n"
                "| Cor | Caramelo natural |\n"
                "| Conservação | Excelente — usada 2x |\n"
                "| Marca | Animale |\n\n"
                "### Destaques\n\n"
                "- **Linho puro** — fibra natural respirável e fresca\n"
                "- Modelagem **wide leg** alongadora\n"
                "- Cintura alta com **passador para cinto**\n"
                "- Tecido **levemente amassado** que valoriza o caimento\n\n"
                "> *A peça que veste bem em qualquer dia de calor — circular é também ser sustentável.*"
            ),
        },
        {
            "name": "Calça Cargo Verde Militar",
            "price": 69.90,
            "discount": 20,
            "featured": False,
            "description": (
                "## Calça Cargo Verde Militar\n\n"
                "Cargo **verde militar autêntica** com bolsões funcionais — "
                "tecido ripstop reforçado, ideal para looks streetwear.\n\n"
                "### Detalhes da Peça\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Tamanho | 40 (cintura: 82cm, comprimento: 100cm) |\n"
                "| Composição | 65% algodão / 35% poliéster |\n"
                "| Cor | Verde militar |\n"
                "| Conservação | Boa — leve uso nos joelhos |\n"
                "| Bolsos | 6 bolsões (2 frente, 2 laterais cargo, 2 traseiros) |\n\n"
                "### Destaques\n\n"
                "- Tecido **ripstop** anti-rasgo\n"
                "- **6 bolsos funcionais** com fechamento por botão\n"
                "- Cordão ajustável nas **barras**\n"
                "- Cintura com **passadores reforçados**\n"
                "- Estilo **utility/streetwear** super em alta\n\n"
                "> *Útil, durável e estilosa — cargo bem usada nunca sai de moda.*"
            ),
        },
    ],
    "Vestidos": [
        {
            "name": "Vestido Midi Floral Anos 70",
            "price": 119.90,
            "discount": 0,
            "featured": True,
            "description": (
                "## Vestido Midi Floral Anos 70\n\n"
                "**Vestido autêntico dos anos 70** com estampa floral em tons quentes — "
                "manga bufante, gola redonda e cintura marcada.\n\n"
                "### Detalhes da Peça\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Tamanho | M (busto: 92cm, cintura: 72cm, comprimento: 110cm) |\n"
                "| Composição | 100% viscose |\n"
                "| Estampa | Floral terracota / mostarda |\n"
                "| Conservação | Muito boa — pequena descosturadura na barra (já reparada) |\n"
                "| Década | 1970 |\n\n"
                "### Destaques\n\n"
                "- **Estampa original setentista** — não é reprodução\n"
                "- Manga **bufante** com elástico no punho\n"
                "- Cintura marcada com **faixa de tecido**\n"
                "- Comprimento **midi** clássico\n"
                "- Cores **terrosas** características da década\n\n"
                "> *Vestir os anos 70 é vestir liberdade — peça de garimpo que volta a circular.*"
            ),
        },
        {
            "name": "Vestido Slip Dress Cetim",
            "price": 79.90,
            "discount": 10,
            "featured": False,
            "description": (
                "## Vestido Slip Dress Cetim\n\n"
                "**Slip dress** em cetim cor champanhe — alças finas, caimento fluido, "
                "elegância dos anos 90 que nunca sai de moda.\n\n"
                "### Detalhes da Peça\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Tamanho | P (busto: 86cm, comprimento: 105cm) |\n"
                "| Composição | 95% poliéster / 5% elastano (cetim) |\n"
                "| Cor | Champanhe |\n"
                "| Conservação | Excelente — peça praticamente nova |\n"
                "| Marca | Zara |\n\n"
                "### Destaques\n\n"
                "- Caimento **fluido em viés** (corte na diagonal)\n"
                "- Alças **finas reguláveis**\n"
                "- Decote **em V** suave\n"
                "- Cor **champanhe** que combina com tudo\n"
                "- Peça **multifuncional**: dia, noite, com tênis ou salto\n\n"
                "> *O vestido coringa que toda mulher quer ter — agora circulando para você.*"
            ),
        },
        {
            "name": "Vestido Tricô Tricolor",
            "price": 99.90,
            "discount": 0,
            "featured": False,
            "description": (
                "## Vestido Tricô Tricolor\n\n"
                "Vestido em **tricô artesanal** com listras tricolor — "
                "bege, marrom e off-white. Caimento confortável para outono/inverno.\n\n"
                "### Detalhes da Peça\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Tamanho | Único — ajuste P/M (busto: 88-96cm) |\n"
                "| Composição | 70% lã / 30% acrílico |\n"
                "| Cor | Tricolor (bege/marrom/off-white) |\n"
                "| Conservação | Boa — sem bolinhas |\n"
                "| Origem | Tricô artesanal brasileiro |\n\n"
                "### Destaques\n\n"
                "- **Tricô artesanal** — feito à mão, peça única\n"
                "- Listras **horizontais alongadoras**\n"
                "- Composição com **70% de lã** natural\n"
                "- Manga **longa** com punho ajustável\n"
                "- Comprimento **midi** versátil\n\n"
                "> *Calor com personalidade — tricô artesanal que vale por dois vestidos comuns.*"
            ),
        },
    ],
    "Casacos": [
        {
            "name": "Jaqueta Jeans Trucker Vintage",
            "price": 159.90,
            "discount": 15,
            "featured": True,
            "description": (
                "## Jaqueta Jeans Trucker Vintage\n\n"
                "**Jaqueta jeans trucker** estilo Levi's clássica — bolsos no peito, "
                "botões de metal, lavagem média com desbote natural.\n\n"
                "### Detalhes da Peça\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Tamanho | M (peito: 108cm, comprimento: 64cm) |\n"
                "| Composição | 100% algodão denim |\n"
                "| Cor | Azul médio com desbote |\n"
                "| Conservação | Boa — desgaste natural nos punhos |\n"
                "| Marca | Levi's Type III |\n\n"
                "### Destaques\n\n"
                "- **Modelo Type III** — o trucker original Levi's\n"
                "- **2 bolsos no peito** com pala\n"
                "- Botões de **metal** com logo Levi's\n"
                "- Punhos com **botão de ajuste**\n"
                "- Caimento **slim** clássico (não oversized)\n\n"
                "> *A jaqueta que toda guarda-roupa precisa ter — atemporal, durável, ícone.*"
            ),
        },
        {
            "name": "Casaco Lã Trench Coat",
            "price": 199.90,
            "discount": 0,
            "featured": False,
            "description": (
                "## Casaco Lã Trench Coat\n\n"
                "**Trench coat** em lã pura cor camel — gola alta, cinto na cintura, "
                "duplo abotoamento. Peça atemporal de garimpo raro.\n\n"
                "### Detalhes da Peça\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Tamanho | M (peito: 100cm, comprimento: 110cm) |\n"
                "| Composição | 80% lã / 20% poliamida |\n"
                "| Cor | Camel clássico |\n"
                "| Conservação | Muito boa — forro intacto |\n"
                "| Origem | Itália (Made in Italy) |\n\n"
                "### Destaques\n\n"
                "- **Lã pura italiana** — caimento e conforto inigualáveis\n"
                "- **Duplo abotoamento** com botões em corozo\n"
                "- **Cinto removível** na cintura\n"
                "- Forro interno em **viscose** acetinada\n"
                "- Comprimento **maxi** elegante\n\n"
                "> *Trench coat de qualidade dura décadas — investimento que circula com estilo.*"
            ),
        },
        {
            "name": "Cardigan Tricô Oversized",
            "price": 89.90,
            "discount": 20,
            "featured": False,
            "description": (
                "## Cardigan Tricô Oversized\n\n"
                "**Cardigan oversized** em tricô grosso cor off-white — "
                "perfeito para sobrepor em qualquer look. Aconchego garantido.\n\n"
                "### Detalhes da Peça\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Tamanho | M/G (busto: 110cm, comprimento: 75cm) |\n"
                "| Composição | 60% acrílico / 40% lã |\n"
                "| Cor | Off-white |\n"
                "| Conservação | Boa — leve desfiamento na barra |\n"
                "| Bolsos | 2 frontais |\n\n"
                "### Destaques\n\n"
                "- Modelagem **oversized** super confortável\n"
                "- Tricô **grosso** com ponto trança\n"
                "- **Botões de madeira** naturais\n"
                "- 2 **bolsões frontais** funcionais\n"
                "- Cor **off-white** que combina com qualquer cor\n\n"
                "> *Conforto em forma de tricô — o cardigan que vira pijama de luxo no inverno.*"
            ),
        },
    ],
    "Acessórios": [
        {
            "name": "Bolsa Couro Caramelo Vintage",
            "price": 179.90,
            "discount": 10,
            "featured": True,
            "description": (
                "## Bolsa Couro Caramelo Vintage\n\n"
                "**Bolsa em couro legítimo** cor caramelo, modelo crossbody médio. "
                "Pátina natural que só o tempo dá, ferragens douradas em ótimo estado.\n\n"
                "### Detalhes da Peça\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Material | Couro bovino legítimo |\n"
                "| Cor | Caramelo (com pátina natural) |\n"
                "| Dimensões | 28cm x 22cm x 10cm |\n"
                "| Alça | Removível, ajustável (60-110cm) |\n"
                "| Conservação | Boa — pátina valoriza a peça |\n\n"
                "### Destaques\n\n"
                "- **Couro legítimo** com cheiro autêntico de couro envelhecido\n"
                "- **Pátina natural** — cada peça é única\n"
                "- Alça **removível e ajustável**\n"
                "- Forro interno em **algodão** com bolso zíper\n"
                "- Ferragens **douradas** sem oxidação\n\n"
                "> *Couro envelhece como vinho — quanto mais usado, mais bonito fica.*"
            ),
        },
        {
            "name": "Cinto Couro Trançado",
            "price": 49.90,
            "discount": 0,
            "featured": False,
            "description": (
                "## Cinto Couro Trançado\n\n"
                "**Cinto em couro trançado** marrom escuro — fivela de metal envelhecido. "
                "Peça versátil que dá charme imediato a qualquer look.\n\n"
                "### Detalhes da Peça\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Material | Couro legítimo trançado |\n"
                "| Largura | 3cm |\n"
                "| Comprimento | 105cm (regulável em vários furos) |\n"
                "| Cor | Marrom escuro |\n"
                "| Conservação | Excelente |\n\n"
                "### Destaques\n\n"
                "- **Tramado artesanal** — couro entrelaçado à mão\n"
                "- Fivela em **metal envelhecido** com pátina\n"
                "- **Múltiplos furos** para ajuste perfeito\n"
                "- Espessura **3cm** versátil (jeans, vestidos, blazer)\n"
                "- Acabamento **costurado à mão**\n\n"
                "> *Detalhe pequeno, impacto grande — cinto certo transforma o look.*"
            ),
        },
        {
            "name": "Lenço Seda Estampado",
            "price": 39.90,
            "discount": 25,
            "featured": False,
            "description": (
                "## Lenço Seda Estampado\n\n"
                "**Lenço quadrado em seda pura** com estampa floral em tons pastéis. "
                "Acessório multifuncional: pescoço, cabelo, bolsa ou cinto.\n\n"
                "### Detalhes da Peça\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Material | 100% seda pura |\n"
                "| Dimensões | 70cm x 70cm |\n"
                "| Estampa | Floral pastel (rosa/azul/verde) |\n"
                "| Conservação | Excelente — sem manchas |\n"
                "| Acabamento | Barra costurada à mão |\n\n"
                "### Destaques\n\n"
                "- **Seda pura** com toque sedoso característico\n"
                "- Estampa **vibrante e fixada** sem desbote\n"
                "- Barra **costurada à mão** (acabamento premium)\n"
                "- **Multifuncional**: 10+ formas de usar\n"
                "- Embala **presente** com elegância\n\n"
                "> *Um lenço de seda é o detalhe que separa o look comum do look memorável.*"
            ),
        },
    ],
}


def slugify(text):
    text = text.lower().strip()
    text = re.sub(r"[àáâãäå]", "a", text)
    text = re.sub(r"[èéêë]", "e", text)
    text = re.sub(r"[ìíîï]", "i", text)
    text = re.sub(r"[òóôõö]", "o", text)
    text = re.sub(r"[ùúûü]", "u", text)
    text = re.sub(r"[ç]", "c", text)
    text = re.sub(r"[^a-z0-9]+", "-", text)
    text = text.strip("-")
    return text


def login():
    print(">> Fazendo login...")
    resp = requests.post(
        f"{NAUTH_URL}/User/loginWithEmail",
        json={"email": NAUTH_EMAIL, "password": NAUTH_PASSWORD},
        headers=COMMON_HEADERS,
    )
    resp.raise_for_status()
    data = resp.json()
    token = data.get("token") or data.get("accessToken")
    if not token:
        print(f"   Erro: resposta inesperada do login: {data}")
        sys.exit(1)
    print(f"   Login OK")
    return token


def create_store(token):
    print(">> Criando store 'Circulou'...")
    resp = requests.post(
        f"{LOFN_URL}/store/insert",
        json={"name": "Circulou"},
        headers={**COMMON_HEADERS, "Authorization": f"Bearer {token}"},
    )
    resp.raise_for_status()
    data = resp.json()
    slug = data["slug"]
    print(f"   Store criada: slug={slug}, id={data['storeId']}")
    return slug


def get_or_create_global_category(token, name, parent_id=None):
    label = f"'{name}'" if parent_id is None else f"'{name}' (parent={parent_id})"
    print(f"   Resolvendo categoria global {label}...")
    auth_headers = {**COMMON_HEADERS, "Authorization": f"Bearer {token}"}

    list_resp = requests.get(
        f"{LOFN_URL}/category-global/list",
        headers=auth_headers,
    )
    list_resp.raise_for_status()
    for item in list_resp.json() or []:
        same_name = (item.get("name") or "").strip().lower() == name.strip().lower()
        same_parent = item.get("parentCategoryId") == parent_id
        if same_name and same_parent:
            print(f"     -> já existe: categoryId={item['categoryId']}")
            return item["categoryId"]

    payload = {"name": name}
    if parent_id is not None:
        payload["parentCategoryId"] = parent_id

    create_resp = requests.post(
        f"{LOFN_URL}/category-global/insert",
        json=payload,
        headers=auth_headers,
    )
    create_resp.raise_for_status()
    data = create_resp.json()
    print(f"     -> nova categoria global: categoryId={data['categoryId']}, slug={data.get('slug')}")
    return data["categoryId"]


def create_product(token, store_slug, category_id, product):
    print(f"   Criando produto '{product['name']}'...")
    resp = requests.post(
        f"{LOFN_URL}/product/{store_slug}/insert",
        json={
            "categoryId": category_id,
            "name": product["name"],
            "description": product["description"],
            "price": product["price"],
            "discount": product.get("discount", 0),
            "frequency": 0,
            "limit": 0,
            "status": 1,  # Active
            "featured": product.get("featured", False),
        },
        headers={**COMMON_HEADERS, "Authorization": f"Bearer {token}"},
    )
    resp.raise_for_status()
    data = resp.json()
    print(f"     -> productId={data['productId']}, slug={data['slug']}")
    return data["productId"], data["slug"]


def generate_image(product_name, slug):
    filepath = os.path.join(PHOTOS_DIR, f"{slug}.png")
    if os.path.exists(filepath):
        print(f"     Imagem já existe: {slug}.png (cache)")
        return filepath

    print(f"     Gerando imagem com DALL-E para '{product_name}'...")
    client = OpenAI(api_key=OPENAI_API_KEY)
    prompt = (
        f"Vintage thrift store clothing photo of {product_name}, "
        "hanging on a wooden hanger against a warm beige textured wall, "
        "soft natural lighting, retro aesthetic, second-hand fashion editorial, "
        "high quality, nostalgic atmosphere"
    )
    response = client.images.generate(
        model="dall-e-2",
        prompt=prompt,
        size="256x256",
        n=1,
    )
    image_url = response.data[0].url
    img_data = requests.get(image_url).content
    with open(filepath, "wb") as f:
        f.write(img_data)
    print(f"     Imagem salva: {slug}.png")
    return filepath


def upload_image(token, product_id, filepath):
    print(f"     Fazendo upload da imagem para productId={product_id}...")
    with open(filepath, "rb") as f:
        resp = requests.post(
            f"{LOFN_URL}/image/upload/{product_id}",
            params={"sortOrder": 0},
            files={"file": (os.path.basename(filepath), f, "image/png")},
            headers={**COMMON_HEADERS, "Authorization": f"Bearer {token}"},
        )
    resp.raise_for_status()
    data = resp.json()
    print(f"     Upload OK: imageId={data['imageId']}")


def main():
    if not all([NAUTH_EMAIL, NAUTH_PASSWORD, NAUTH_URL, LOFN_URL, OPENAI_API_KEY]):
        print("Erro: configure todas as variáveis no arquivo .env")
        sys.exit(1)

    masked_key = f"{OPENAI_API_KEY[:3]}...{OPENAI_API_KEY[-4:]}"
    print(f">> OpenAI API Key: {masked_key}")

    # Fase 1: Gerar todas as imagens via DALL-E
    print("\n========== FASE 1: Gerando imagens ==========\n")
    image_map = {}
    for category_name, products in CATEGORIES.items():
        print(f">> Categoria: {category_name}")
        for product in products:
            slug = slugify(product["name"])
            filepath = generate_image(product["name"], slug)
            image_map[product["name"]] = filepath

    # Fase 2: Criar dados na API do Lofn
    print("\n========== FASE 2: Criando dados na API ==========\n")
    token = login()
    store_slug = create_store(token)

    print(f"\n>> Categoria raiz: {ROOT_CATEGORY}")
    root_category_id = get_or_create_global_category(token, ROOT_CATEGORY)

    for category_name, products in CATEGORIES.items():
        print(f"\n>> Subcategoria: {ROOT_CATEGORY}/{category_name}")
        category_id = get_or_create_global_category(token, category_name, parent_id=root_category_id)

        for product in products:
            product_id, product_slug = create_product(
                token, store_slug, category_id, product
            )

            filepath = image_map[product["name"]]
            upload_image(token, product_id, filepath)

    print("\n>> Seed completo!")
    print(f"   Store: {store_slug}")
    print(f"   Categoria raiz: {ROOT_CATEGORY}")
    print(f"   Subcategorias: {len(CATEGORIES)}")
    print(f"   Produtos: {sum(len(p) for p in CATEGORIES.values())}")


if __name__ == "__main__":
    main()

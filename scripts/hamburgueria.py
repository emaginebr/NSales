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

PHOTOS_DIR = os.path.join(os.path.dirname(__file__), "photos", "hamburgueria")
os.makedirs(PHOTOS_DIR, exist_ok=True)

ROOT_CATEGORY = "Cardápio"

CATEGORIES = {
    "Sanduiches": [
        {
            "name": "Smash Burger Duplo",
            "price": 34.90,
            "discount": 0,
            "featured": True,
            "description": (
                "## Smash Burger Duplo\n\n"
                "O clássico **Smash Burger** com dois discos de carne **smashados** na chapa, "
                "queijo derretido e molho especial da casa.\n\n"
                "### Ingredientes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Pão | Brioche artesanal tostado na manteiga |\n"
                "| Carne | 2x 90g blend bovino (fraldinha + peito) |\n"
                "| Queijo | Cheddar americano derretido |\n"
                "| Molho | Smash sauce da casa |\n"
                "| Extras | Cebola caramelizada, pickles |\n\n"
                "### Destaques\n\n"
                "- Carne **smashada** na chapa em alta temperatura para crosta crocante\n"
                "- Pão **brioche artesanal** feito diariamente\n"
                "- Blend exclusivo de **fraldinha + peito** moído na hora\n"
                "- Queijo cheddar derretido **na carne** durante o preparo\n\n"
                "> *Nosso carro-chefe — simples, suculento e impossível de resistir.*"
            ),
        },
        {
            "name": "Burger Bacon Crispy",
            "price": 39.90,
            "discount": 10,
            "featured": True,
            "description": (
                "## Burger Bacon Crispy\n\n"
                "Para os amantes de bacon: camadas generosas de **bacon crocante** "
                "com carne suculenta e cebola crispy.\n\n"
                "### Ingredientes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Pão | Brioche com gergelim |\n"
                "| Carne | 1x 180g blend angus |\n"
                "| Bacon | 3 fatias crocantes defumadas |\n"
                "| Queijo | Provolone maçaricado |\n"
                "| Extras | Cebola crispy, alface, tomate, maionese defumada |\n\n"
                "### Destaques\n\n"
                "- Bacon **artesanal defumado** com madeira de macieira\n"
                "- Provolone **maçaricado** na hora para gratinar\n"
                "- Cebola crispy feita em **tempurá leve**\n"
                "- Maionese defumada com **páprica e chipotle**\n\n"
                "> *Crocância em cada mordida — bacon de verdade faz toda a diferença.*"
            ),
        },
        {
            "name": "Veggie Burger",
            "price": 32.90,
            "discount": 0,
            "featured": False,
            "description": (
                "## Veggie Burger\n\n"
                "Nosso hambúrguer **100% vegetal**, feito com grão-de-bico, "
                "beterraba e especiarias — saboroso e sem nada de origem animal.\n\n"
                "### Ingredientes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Pão | Integral com sementes |\n"
                "| Burger | 150g (grão-de-bico, beterraba, aveia) |\n"
                "| Queijo | Muçarela de castanha de caju |\n"
                "| Molho | Tahine com limão siciliano |\n"
                "| Extras | Rúcula, tomate seco, cebola roxa |\n\n"
                "### Destaques\n\n"
                "- **100% vegetal** — sem carne, ovos ou laticínios\n"
                "- Disco feito artesanalmente com **grão-de-bico e beterraba**\n"
                "- Pão integral com **sementes de girassol e linhaça**\n"
                "- Molho de **tahine** com toque cítrico\n\n"
                "> *Prova de que sabor não precisa de carne — surpreende até os carnívoros.*"
            ),
        },
    ],
    "Acompanhamentos": [
        {
            "name": "Batata Frita Rústica",
            "price": 18.90,
            "discount": 0,
            "featured": True,
            "description": (
                "## Batata Frita Rústica\n\n"
                "Batatas cortadas com casca em formato **wedge**, fritas até "
                "ficarem douradas e temperadas com alho e ervas.\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Porção | 300g |\n"
                "| Corte | Wedge (gomos com casca) |\n"
                "| Tempero | Alho, alecrim, sal grosso, páprica |\n"
                "| Acompanha | Maionese temperada da casa |\n\n"
                "### Destaques\n\n"
                "- Cortadas à mão em formato **wedge** rústico\n"
                "- Fritas em **duas etapas** para interior macio e exterior crocante\n"
                "- Temperadas com **alho confitado** e alecrim fresco\n"
                "- Acompanha **maionese da casa** com ervas finas\n\n"
                "> *Crocante por fora, macia por dentro — a batata como deve ser.*"
            ),
        },
        {
            "name": "Onion Rings",
            "price": 16.90,
            "discount": 15,
            "featured": False,
            "description": (
                "## Onion Rings\n\n"
                "Anéis de cebola empanados em massa **beer batter** "
                "crocante, servidos com molho barbecue.\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Porção | 250g (aprox. 12 anéis) |\n"
                "| Empanamento | Beer batter com cerveja artesanal |\n"
                "| Acompanha | Molho barbecue defumado |\n\n"
                "### Destaques\n\n"
                "- Massa **beer batter** com cerveja IPA local\n"
                "- Cebola **doce roxa** selecionada\n"
                "- Empanamento leve e **extra crocante**\n"
                "- Molho barbecue com **melaço e defumação natural**\n\n"
                "> *O acompanhamento clássico americano — impossível comer só um.*"
            ),
        },
        {
            "name": "Mac & Cheese",
            "price": 22.90,
            "discount": 0,
            "featured": False,
            "description": (
                "## Mac & Cheese\n\n"
                "Macarrão cotovelo com molho **cremoso de três queijos**, "
                "gratinado com crosta de parmesão.\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Porção | 250g |\n"
                "| Massa | Cotovelo al dente |\n"
                "| Queijos | Cheddar, gruyère, parmesão |\n"
                "| Finalização | Gratinado com parmesão e breadcrumbs |\n\n"
                "### Destaques\n\n"
                "- Molho com **três queijos** derretidos em creme de leite fresco\n"
                "- Gratinado no forno com crosta de **parmesão e breadcrumbs**\n"
                "- Massa **al dente** que segura o molho cremoso\n"
                "- Receita inspirada no **comfort food americano** clássico\n\n"
                "> *Conforto em forma de comida — cremoso, gratinado e irresistível.*"
            ),
        },
    ],
    "Sobremesas": [
        {
            "name": "Brownie com Sorvete",
            "price": 24.90,
            "discount": 0,
            "featured": True,
            "description": (
                "## Brownie com Sorvete\n\n"
                "Brownie de chocolate **70% cacau** servido quente com "
                "bola de sorvete de baunilha e calda de chocolate.\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Brownie | 120g, chocolate 70% cacau |\n"
                "| Sorvete | 1 bola de baunilha artesanal |\n"
                "| Calda | Chocolate belga quente |\n"
                "| Topping | Nibs de cacau e chantilly |\n\n"
                "### Destaques\n\n"
                "- Brownie feito com **chocolate 70% cacau** belga\n"
                "- Servido **quente** com centro levemente cremoso\n"
                "- Sorvete de **baunilha artesanal** com favas reais\n"
                "- Calda de chocolate quente por cima\n\n"
                "> *O contraste quente-frio perfeito — chocolate em seu estado mais puro.*"
            ),
        },
        {
            "name": "Milkshake Nutella",
            "price": 19.90,
            "discount": 20,
            "featured": False,
            "description": (
                "## Milkshake Nutella\n\n"
                "Milkshake cremoso de **Nutella** batido com sorvete de creme "
                "e finalizado com chantilly e raspas de chocolate.\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Volume | 400ml |\n"
                "| Base | Sorvete de creme + leite integral |\n"
                "| Sabor | Nutella (2 colheres generosas) |\n"
                "| Topping | Chantilly, raspas de chocolate, canudo wafer |\n\n"
                "### Destaques\n\n"
                "- Batido com **Nutella de verdade**, não aroma\n"
                "- Textura **ultra cremosa** com sorvete artesanal\n"
                "- Servido em copo de vidro com **chantilly** fresco\n"
                "- Canudo **wafer** comestível de chocolate\n\n"
                "> *Indulgência pura em cada gole — Nutella lovers, este é o seu.*"
            ),
        },
        {
            "name": "Churros Recheados",
            "price": 14.90,
            "discount": 0,
            "featured": False,
            "description": (
                "## Churros Recheados\n\n"
                "Churros artesanais crocantes por fora, macios por dentro, "
                "recheados com **doce de leite** e cobertos com açúcar e canela.\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Porção | 3 unidades |\n"
                "| Recheio | Doce de leite argentino |\n"
                "| Cobertura | Açúcar com canela |\n"
                "| Acompanha | Calda de chocolate para dipping |\n\n"
                "### Destaques\n\n"
                "- Massa de churros feita **na hora**, frita sob demanda\n"
                "- Recheados com **doce de leite argentino** importado\n"
                "- Cobertura de **açúcar e canela** do Ceilão\n"
                "- Acompanha potinho de **calda de chocolate** para mergulhar\n\n"
                "> *Crocante, quente e recheado — o doce que todo mundo pede de novo.*"
            ),
        },
    ],
    "Bebidas": [
        {
            "name": "Limonada Suíça",
            "price": 12.90,
            "discount": 0,
            "featured": False,
            "description": (
                "## Limonada Suíça\n\n"
                "Limonada batida com **limão siciliano**, leite condensado "
                "e gelo — cremosa e refrescante.\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Volume | 500ml |\n"
                "| Limão | Siciliano fresco |\n"
                "| Base | Leite condensado + água com gás |\n"
                "| Servido | Copo long drink com gelo |\n\n"
                "### Destaques\n\n"
                "- Feita com **limão siciliano** fresco espremido na hora\n"
                "- Toque cremoso do **leite condensado** artesanal\n"
                "- Opção com **água com gás** para mais frescor\n"
                "- Servida bem **gelada** com rodela de limão\n\n"
                "> *Refrescância cremosa — a limonada que só a casa faz assim.*"
            ),
        },
        {
            "name": "Suco Natural de Laranja",
            "price": 10.90,
            "discount": 0,
            "featured": False,
            "description": (
                "## Suco Natural de Laranja\n\n"
                "Suco de laranja **espremido na hora**, sem adição de "
                "açúcar, água ou conservantes — pura fruta.\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Volume | 400ml |\n"
                "| Laranja | Pera ou Lima (safra do dia) |\n"
                "| Açúcar | Sem adição (opcional à parte) |\n"
                "| Servido | Copo com gelo |\n\n"
                "### Destaques\n\n"
                "- **100% natural** — espremido no momento do pedido\n"
                "- Sem adição de **água, açúcar ou conservantes**\n"
                "- Laranjas selecionadas da **safra do dia**\n"
                "- Opção de adoçar com **mel orgânico** (R$ 2,00)\n\n"
                "> *Simples e honesto — suco de laranja como deveria ser.*"
            ),
        },
        {
            "name": "Iced Tea Pêssego",
            "price": 11.90,
            "discount": 10,
            "featured": True,
            "description": (
                "## Iced Tea Pêssego\n\n"
                "Chá gelado de **pêssego** feito com chá preto infusionado "
                "e polpa de pêssego natural — leve e refrescante.\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Volume | 500ml |\n"
                "| Chá | Preto (Darjeeling) infusionado a frio |\n"
                "| Fruta | Polpa de pêssego natural |\n"
                "| Açúcar | Levemente adoçado com mel |\n"
                "| Servido | Copo long drink com gelo e fatia de pêssego |\n\n"
                "### Destaques\n\n"
                "- Chá preto **infusionado a frio** por 12 horas (cold brew)\n"
                "- Polpa de **pêssego natural** sem conservantes\n"
                "- Adoçado suavemente com **mel orgânico**\n"
                "- Decorado com **fatia de pêssego** fresco\n\n"
                "> *Leve, frutado e gelado — o par perfeito para qualquer burger.*"
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
    print(">> Criando store 'Hamburgueria Artesanal'...")
    resp = requests.post(
        f"{LOFN_URL}/store/insert",
        json={"name": "Hamburgueria Artesanal"},
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
        f"Professional food photography of {product_name}, "
        "top-down view, dark wood table background, moody lighting, "
        "restaurant menu style, high quality, appetizing"
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

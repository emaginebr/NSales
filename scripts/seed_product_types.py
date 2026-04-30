"""
Helpers de seed para Product Types, Filters, Customizations e linking com categorias.

Uso típico:

    from seed_product_types import seed_product_type_tree, link_category_to_product_type

    spec = {
        "name": "Comida",
        "filters": [
            {"label": "Tamanho", "data_type": "enum", "allowed_values": ["P", "M", "G"]},
            {"label": "Vegetariano", "data_type": "boolean"},
        ],
        "customization_groups": [
            {
                "label": "Adicionais",
                "selection_mode": "multi",
                "options": [
                    {"label": "Bacon Extra", "price_delta_cents": 500},
                    {"label": "Queijo Extra", "price_delta_cents": 300},
                ],
            },
        ],
    }
    type_id = seed_product_type_tree(token, headers, lofn_url, spec)
    link_category_to_product_type(token, headers, lofn_url, category_id, type_id, marketplace=True)

Os endpoints REST utilizados são todos `[TenantAdmin]` (exigem `IsAdmin = true`).
Linking usa `/category-global/{id}/producttype/{typeId}` quando `marketplace=True`,
ou `/category/{id}/producttype/{typeId}` caso contrário.
"""

import requests


def _auth(headers, token):
    return {**headers, "Authorization": f"Bearer {token}"}


def get_or_create_product_type(token, headers, lofn_url, name):
    """Retorna productTypeId — reusa se já existir, cria caso contrário."""
    print(f"   Resolvendo product type '{name}'...")
    auth_headers = _auth(headers, token)

    list_resp = requests.get(f"{lofn_url}/producttype/list", headers=auth_headers)
    list_resp.raise_for_status()
    for item in list_resp.json() or []:
        if (item.get("name") or "").strip().lower() == name.strip().lower():
            print(f"     -> já existe: productTypeId={item['productTypeId']}")
            return item["productTypeId"]

    create_resp = requests.post(
        f"{lofn_url}/producttype/insert",
        json={"name": name},
        headers=auth_headers,
    )
    create_resp.raise_for_status()
    data = create_resp.json()
    print(f"     -> novo product type: productTypeId={data['productTypeId']}")
    return data["productTypeId"]


def get_product_type(token, headers, lofn_url, product_type_id):
    """GET /producttype/{id} — retorna árvore completa (filters + customizationGroups + options)."""
    auth_headers = _auth(headers, token)
    resp = requests.get(f"{lofn_url}/producttype/{product_type_id}", headers=auth_headers)
    resp.raise_for_status()
    return resp.json()


def add_filter(
    token,
    headers,
    lofn_url,
    product_type_id,
    label,
    data_type,
    allowed_values=None,
    is_required=False,
    display_order=0,
):
    """
    Adiciona filtro a um product type. data_type ∈ {text, integer, decimal, boolean, enum}.
    Para data_type='enum', passe allowed_values como lista de strings.
    """
    payload = {
        "label": label,
        "dataType": data_type,
        "isRequired": is_required,
        "displayOrder": display_order,
    }
    if allowed_values is not None:
        payload["allowedValues"] = list(allowed_values)

    resp = requests.post(
        f"{lofn_url}/producttype/{product_type_id}/filter/insert",
        json=payload,
        headers=_auth(headers, token),
    )
    resp.raise_for_status()
    data = resp.json()
    print(f"     + filter '{label}' ({data_type}): filterId={data['filterId']}")
    return data["filterId"]


def update_filter(
    token,
    headers,
    lofn_url,
    filter_id,
    label,
    is_required,
    display_order,
    allowed_values=None,
):
    """
    POST /producttype/filter/update — DataType é imutável, label/isRequired/displayOrder/allowedValues podem ser alterados.
    """
    payload = {
        "filterId": filter_id,
        "label": label,
        "isRequired": is_required,
        "displayOrder": display_order,
    }
    if allowed_values is not None:
        payload["allowedValues"] = list(allowed_values)
    resp = requests.post(
        f"{lofn_url}/producttype/filter/update",
        json=payload,
        headers=_auth(headers, token),
    )
    resp.raise_for_status()
    print(f"     ~ filter '{label}' atualizado (isRequired={is_required})")


def add_customization_group(
    token,
    headers,
    lofn_url,
    product_type_id,
    label,
    selection_mode,
    is_required=False,
    display_order=0,
):
    """selection_mode ∈ {single, multi}."""
    resp = requests.post(
        f"{lofn_url}/producttype/{product_type_id}/customization/group/insert",
        json={
            "label": label,
            "selectionMode": selection_mode,
            "isRequired": is_required,
            "displayOrder": display_order,
        },
        headers=_auth(headers, token),
    )
    resp.raise_for_status()
    data = resp.json()
    print(f"     + group '{label}' ({selection_mode}, required={is_required}): groupId={data['groupId']}")
    return data["groupId"]


def add_customization_option(
    token,
    headers,
    lofn_url,
    group_id,
    label,
    price_delta_cents=0,
    is_default=False,
    display_order=0,
):
    resp = requests.post(
        f"{lofn_url}/producttype/customization/group/{group_id}/option/insert",
        json={
            "label": label,
            "priceDeltaCents": price_delta_cents,
            "isDefault": is_default,
            "displayOrder": display_order,
        },
        headers=_auth(headers, token),
    )
    resp.raise_for_status()
    data = resp.json()
    print(
        f"       · option '{label}' (+{price_delta_cents/100:.2f}, default={is_default}): "
        f"optionId={data['optionId']}"
    )
    return data["optionId"]


def link_category_to_product_type(
    token, headers, lofn_url, category_id, product_type_id, marketplace=True
):
    base = "category-global" if marketplace else "category"
    path = f"{lofn_url}/{base}/{category_id}/producttype/{product_type_id}"
    resp = requests.put(path, headers=_auth(headers, token))
    resp.raise_for_status()
    print(f"   ↪ link {base}/{category_id} → productType/{product_type_id} OK")


def seed_product_type_tree(token, headers, lofn_url, spec):
    """
    Cria (ou reusa) um product type completo a partir de um dict-spec:

        {
            "name": "Comida",
            "filters": [
                {"label": "Tamanho", "data_type": "enum", "allowed_values": [...], "is_required": False},
                ...
            ],
            "customization_groups": [
                {
                    "label": "Adicionais",
                    "selection_mode": "multi",
                    "is_required": False,
                    "options": [
                        {"label": "Bacon Extra", "price_delta_cents": 500, "is_default": False},
                        ...
                    ],
                },
                ...
            ],
        }

    Notas:
    - Se o type já existe (reuso por nome), filtros/groups NÃO são re-inseridos. A função
      compara nomes para evitar duplicação.
    - Retorna o productTypeId.
    """
    type_id = get_or_create_product_type(token, headers, lofn_url, spec["name"])

    existing = get_product_type(token, headers, lofn_url, type_id)
    existing_filters_by_label = {
        (f.get("label") or "").strip().lower(): f
        for f in (existing.get("filters") or [])
    }
    existing_group_labels = {
        (g.get("label") or "").strip().lower(): g
        for g in (existing.get("customizationGroups") or [])
    }

    for i, f in enumerate(spec.get("filters") or []):
        label_key = f["label"].strip().lower()
        spec_required = f.get("is_required", False)
        spec_order = f.get("display_order", i)
        if label_key in existing_filters_by_label:
            existing_f = existing_filters_by_label[label_key]
            existing_required = bool(existing_f.get("isRequired", False))
            existing_order = int(existing_f.get("displayOrder", 0))
            if existing_required != spec_required or existing_order != spec_order:
                update_filter(
                    token,
                    headers,
                    lofn_url,
                    filter_id=existing_f["filterId"],
                    label=f["label"],
                    is_required=spec_required,
                    display_order=spec_order,
                    allowed_values=f.get("allowed_values"),
                )
            else:
                print(f"     · filter '{f['label']}' já está sincronizado — pulando")
            continue
        add_filter(
            token,
            headers,
            lofn_url,
            type_id,
            label=f["label"],
            data_type=f["data_type"],
            allowed_values=f.get("allowed_values"),
            is_required=spec_required,
            display_order=spec_order,
        )

    for gi, g in enumerate(spec.get("customization_groups") or []):
        key = g["label"].strip().lower()
        if key in existing_group_labels:
            print(f"     · group '{g['label']}' já existe — pulando criação (e suas options)")
            continue
        group_id = add_customization_group(
            token,
            headers,
            lofn_url,
            type_id,
            label=g["label"],
            selection_mode=g["selection_mode"],
            is_required=g.get("is_required", False),
            display_order=g.get("display_order", gi),
        )
        for oi, o in enumerate(g.get("options") or []):
            add_customization_option(
                token,
                headers,
                lofn_url,
                group_id,
                label=o["label"],
                price_delta_cents=o.get("price_delta_cents", 0),
                is_default=o.get("is_default", False),
                display_order=o.get("display_order", oi),
            )

    return type_id

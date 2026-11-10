namespace AmazonCreators.Net.Tests.Fakes;

public static class MockCreatorsPayloads
{
    public const string StandardGetItemsResponse = """
    {
      "itemsResult": {
        "items": [
          {
            "asin": "B08N5WRWNW",
            "detailPageURL": "https://www.amazon.es/dp/B08N5WRWNW?tag=testtag-21",
            "itemInfo": {
              "title": {
                "displayValue": "Sony PlayStation 5 Console Standard Edition",
                "locale": "es_ES"
              },
              "byLineInfo": {
                "brand": {
                  "displayValue": "Sony Interactive Entertainment",
                  "locale": "es_ES"
                }
              },
              "externalIds": {
                "EANs": {
                  "displayValues": ["0711719541028"]
                }
              }
            },
            "images": {
              "primary": {
                "large": {
                  "url": "https://m.media-amazon.com/images/I/71v1Q6kX5PL._AC_SL1500_.jpg",
                  "height": 1500,
                  "width": 1500
                },
                "highRes": {
                  "url": "https://m.media-amazon.com/images/I/71v1Q6kX5PL._AC_SL2000_.jpg",
                  "height": 2000,
                  "width": 2000
                }
              }
            },
            "offersV2": {
              "listings": [
                {
                  "price": {
                    "money": {
                      "amount": 499.99,
                      "currency": "EUR",
                      "displayAmount": "499,99 €"
                    },
                    "savingBasis": {
                      "money": {
                        "amount": 549.99,
                        "currency": "EUR",
                        "displayAmount": "549,99 €"
                      },
                      "savingBasisType": "LIST_PRICE",
                      "savingBasisTypeLabel": "Precio recomendado"
                    },
                    "savings": {
                      "money": {
                        "amount": 50.00,
                        "currency": "EUR",
                        "displayAmount": "50,00 €"
                      },
                      "percentage": 9.09
                    }
                  },
                  "availability": {
                    "type": "IN_STOCK",
                    "message": "En stock"
                  },
                  "condition": {
                    "value": "New"
                  },
                  "merchantInfo": {
                    "id": "A1ATVPDKIKX0DER",
                    "name": "Amazon"
                  },
                  "isBuyBoxWinner": true
                },
                {
                  "price": {
                    "money": {
                      "amount": 520.00,
                      "currency": "EUR",
                      "displayAmount": "520,00 €"
                    }
                  },
                  "availability": {
                    "type": "IN_STOCK",
                    "message": "En stock"
                  },
                  "condition": {
                    "value": "New"
                  },
                  "merchantInfo": {
                    "id": "A2VXZQ38EXAMPLE",
                    "name": "Third Party Seller"
                  },
                  "isBuyBoxWinner": false
                }
              ]
            }
          }
        ]
      }
    }
    """;

    public const string StandardSearchItemsResponse = """
    {
      "searchResult": {
        "totalResultCount": 1,
        "searchURL": "https://www.amazon.es/s?k=0711719541028",
        "items": [
          {
            "asin": "B08N5WRWNW",
            "detailPageURL": "https://www.amazon.es/dp/B08N5WRWNW",
            "itemInfo": {
              "title": {
                "displayValue": "Sony PlayStation 5 Console Standard Edition",
                "locale": "es_ES"
              },
              "externalIds": {
                "EANs": {
                  "displayValues": ["0711719541028"]
                }
              }
            }
          }
        ]
      }
    }
    """;
}

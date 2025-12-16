using MadPixel;
using MadPixel.InApps;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [SerializeField] private Button[] buy_buttons;

    [SerializeField] private InputField InputField;

    private int player_code;

    private string[] products_id = new string[12]
    {
        "money_500k",
        "money_1m",
        "money_3m",
        "spinner",
        "grenade_launcher",
        "money_x2",
        "missile_strike",
        "speed_x3",
        "no_ads",
        "laser_mg",
        "radiation",
        "laser_gun"
    };

    private int[] costs = new int[12]
    {
        0,
        0,
        0,
        1250000,
        2000000,
        0,
        750000,
        800000,
        0,
        1250000,
        1000000,
        2000000
    };

    [SerializeField] private Text money_text;

    [SerializeField] private Transform canvas;

    [SerializeField] private float clamp0 = 1000;
    private float clamp1;

    [SerializeField] private RectTransform buttons_panel;

    [SerializeField] private GameObject bought_panel;

    private float hold_last_pos;
    private float hold_start_pos;

    private Vector2 target_position;

    public static Shop instance;
    private void Awake()
    {
        instance = this;
    }
    private static System.Action<Product> _onPurchaseResult;

    private void Start()
    {
        _onPurchaseResult += OnPurchaseResult;


        FirebaseManager.instance.logEvent("visit_shop");
        money_text.text = Player.instance.get_money();


        clamp0 *= canvas.localScale.x;
        clamp1 = buttons_panel.transform.position.y;
        target_position = buttons_panel.transform.position;

        for (int i = 0; i < buy_buttons.Length; i++)
        {
            // For non-consumable products, check if already purchased
            if (products_id[i] == "no_ads" && MobileInAppPurchaser.HasReceipt(products_id[i]))
            {
                PlayerPrefs.SetInt("no_ads_on", 1);
                var panel = Instantiate(bought_panel, buy_buttons[i].transform.parent);
                panel.transform.localPosition = Vector3.zero;
                Destroy(buy_buttons[i].gameObject);
                continue;
            }

            if (PlayerPrefs.GetInt("bought_" + i) == 1)
            {
                var panel = Instantiate(bought_panel, buy_buttons[i].transform.parent);
                panel.transform.localPosition = Vector3.zero;
                Destroy(buy_buttons[i].gameObject);
            }
            else
            {
                buy_buttons[i].onClick.AddListener(button_pressed);

                // Set price tags for IAP products
                if (costs[i] == 0 && !allfree)
                {
                    Product p = MobileInAppPurchaser.Instance.GetProduct(products_id[i]);
                    if (p != null)
                    {
                        Text priceText = buy_buttons[i].GetComponentInChildren<Text>();
                        if (priceText != null)
                        {
                            priceText.text = p.metadata.localizedPriceString;
                        }
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (MobileInAppPurchaser.Instance != null)
        {
            MobileInAppPurchaser.Instance.OnPurchaseResult -= OnPurchaseResult;
        }
        _onPurchaseResult -= OnPurchaseResult;
    }

    private void OnPurchaseResult(Product product)
    {
        if (product != null)
        {
            Debug.Log($"Purchase complete! Product ID: {product.definition.id}");
            if (product.definition.id == MobileInAppPurchaser.AdsFreeSKU)
            {
                AdsManager.CancelAllAds();
                // сохраните, что AdsFree куплен
            }
            give_product(product.definition.id);
        }
        else
        {
            Debug.LogError("Purchase failed or was canceled!");
        }
    }

    private void checkCode(string text)
    {

    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            hold_last_pos = Input.mousePosition.y;
            hold_start_pos = hold_last_pos;
        }
        else if (Input.GetMouseButton(0))
        {
            var delta = Input.mousePosition.y - hold_last_pos;
            var new_position = target_position + new Vector2(0, delta);
            if (new_position.y >= clamp1 && new_position.y <= clamp0)
                target_position = new_position;
            hold_last_pos = Input.mousePosition.y;
        }
        else if (Input.GetMouseButtonUp(0))
        {

        }

        if ((Vector2)buttons_panel.transform.position != target_position)
            buttons_panel.transform.position = Vector2.Lerp(buttons_panel.transform.position, target_position, .2f);
    }
    public static void HandleIAPPurchaseResult(Product product)
    {
        // Вызываем событие для всех активных Shop
        _onPurchaseResult?.Invoke(product);
    }

    private void button_pressed()
    {
        int button_index = int.Parse(EventSystem.current.currentSelectedGameObject.name.Substring(4));
        buy(button_index);
        money_text.text = Player.instance.get_money();
    }

    [SerializeField] private bool allfree = false;

    private void buy(int index)
    {
        if (Mathf.Abs(hold_start_pos - Input.mousePosition.y) < 10)
        {
            print("buy" + index);
            if (costs[index] != 0)
            {
                if (Player.instance.add_money(-costs[index]))
                {
                    give_product(products_id[index]);
                }
            }
            else if (!allfree)
            {
                MobileInAppPurchaser.BuyProduct(products_id[index]);
            }
            else
            {
                give_product(products_id[index]);
            }
        }
    }

    public void give_product(string id)
    {
        if (id == "money_500k")
        {
            Player.instance.add_money(500000);
            money_text.text = Player.instance.get_money();
            //if (!allfree) FirebaseManager.instance.logEvent("buy_money_500k");
        }
        else if (id == "money_1m")
        {
            Player.instance.add_money(1000000);
            money_text.text = Player.instance.get_money();
            //if (!allfree) FirebaseManager.instance.logEvent("buy_money_1m");
        }
        else if (id == "money_3m")
        {
            Player.instance.add_money(3000000);
            money_text.text = Player.instance.get_money();
            //if (!allfree) FirebaseManager.instance.logEvent("buy_money_3m");
        }
        else if (id == "spinner")
        {
            int button_index = System.Array.IndexOf(products_id, id);
            PlayerPrefs.SetInt("tower_5_level", 3);
            var panel = Instantiate(bought_panel, buy_buttons[button_index].transform.parent);
            panel.transform.localPosition = Vector3.zero;
            PlayerPrefs.SetInt("bought_" + button_index, 1);
            Destroy(buy_buttons[button_index].gameObject);
            // FirebaseManager.instance.logEvent("buy_spinner");
            LevelLoader.instance.loadScene("show_spinner");

        }
        else if (id == "grenade_launcher")
        {
            int button_index = System.Array.IndexOf(products_id, id);
            PlayerPrefs.SetInt("tower_6_level", 3);
            var panel = Instantiate(bought_panel, buy_buttons[button_index].transform.parent);
            panel.transform.localPosition = Vector3.zero;
            PlayerPrefs.SetInt("bought_" + button_index, 1);
            Destroy(buy_buttons[button_index].gameObject);
            // FirebaseManager.instance.logEvent("buy_grenade_launcher");
            LevelLoader.instance.loadScene("show_gl");
        }
        else if (id == "laser_mg")
        {
            int button_index = System.Array.IndexOf(products_id, id);
            PlayerPrefs.SetInt("tower_8_level", 3);
            var panel = Instantiate(bought_panel, buy_buttons[button_index].transform.parent);
            panel.transform.localPosition = Vector3.zero;
            PlayerPrefs.SetInt("bought_" + button_index, 1);
            Destroy(buy_buttons[button_index].gameObject);
            // FirebaseManager.instance.logEvent("buy_laser_mg");
            LevelLoader.instance.loadScene("show_lmg");

        }
        else if (id == "radiation")
        {
            int button_index = System.Array.IndexOf(products_id, id);
            PlayerPrefs.SetInt("tower_7_level", 3);
            var panel = Instantiate(bought_panel, buy_buttons[button_index].transform.parent);
            panel.transform.localPosition = Vector3.zero;
            PlayerPrefs.SetInt("bought_" + button_index, 1);
            Destroy(buy_buttons[button_index].gameObject);
            // FirebaseManager.instance.logEvent("buy_radiation");
            LevelLoader.instance.loadScene("show_radiation");
        }
        else if (id == "laser_gun")
        {
            int button_index = System.Array.IndexOf(products_id, id);
            PlayerPrefs.SetInt("tower_9_level", 2);
            var panel = Instantiate(bought_panel, buy_buttons[button_index].transform.parent);
            panel.transform.localPosition = Vector3.zero;
            PlayerPrefs.SetInt("bought_" + button_index, 1);
            Destroy(buy_buttons[button_index].gameObject);
            // FirebaseManager.instance.logEvent("buy_laser_gun");
            LevelLoader.instance.loadScene("show_lg");
        }
        else if (id == "money_x2")
        {
            int button_index = System.Array.IndexOf(products_id, id);
            print(button_index);
            PlayerPrefs.SetInt("money_x2_on", 1);
            var panel = Instantiate(bought_panel, buy_buttons[button_index].transform.parent);
            panel.transform.localPosition = Vector3.zero;
            PlayerPrefs.SetInt("bought_" + button_index, 1);
            Destroy(buy_buttons[button_index].gameObject);
            // if (!allfree) FirebaseManager.instance.logEvent("buy_money_x2");
        }
        else if (id == "missile_strike")
        {
            int button_index = System.Array.IndexOf(products_id, id);
            PlayerPrefs.SetInt("missile_strike_on", 1);
            var panel = Instantiate(bought_panel, buy_buttons[button_index].transform.parent);
            panel.transform.localPosition = Vector3.zero;
            PlayerPrefs.SetInt("bought_" + button_index, 1);
            Destroy(buy_buttons[button_index].gameObject);
            // FirebaseManager.instance.logEvent("buy_missile_strike");
        }
        else if (id == "speed_x3")
        {
            FirebaseManager.instance.on_ad = false;
            int button_index = System.Array.IndexOf(products_id, id);
            PlayerPrefs.SetInt("speed_x3_on", 1);
            var panel = Instantiate(bought_panel, buy_buttons[button_index].transform.parent);
            panel.transform.localPosition = Vector3.zero;
            PlayerPrefs.SetInt("bought_" + button_index, 1);
            Destroy(buy_buttons[button_index].gameObject);
            // FirebaseManager.instance.logEvent("buy_speed_x3");
        }
        else if (id == "no_ads")
        {
            int button_index = System.Array.IndexOf(products_id, id);
            PlayerPrefs.SetInt("no_ads_on", 1);
            var panel = Instantiate(bought_panel, buy_buttons[button_index].transform.parent);
            panel.transform.localPosition = Vector3.zero;
            PlayerPrefs.SetInt("bought_" + button_index, 1);
            Destroy(buy_buttons[button_index].gameObject);
            // if (!allfree) FirebaseManager.instance.logEvent("buy_no_ads");
            FirebaseManager.instance.on_ad = false;
        }
    }

}


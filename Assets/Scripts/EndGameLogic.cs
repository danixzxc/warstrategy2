using MadPixel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndGameLogic : MonoBehaviour
{
    [SerializeField] private Color[] stars_colors;

    [SerializeField] private Image[] stars;

    [SerializeField] private Text coins_text;

    [SerializeField] private Text reward_text;

    [SerializeField] private Button restart_button;

    [SerializeField] private Button next_button;

    [SerializeField] private Button home_button;

    [SerializeField] private Animator animator;

    [SerializeField] private Text reward_coins_text;

    [SerializeField] private Text player_money_text;

    private int reward = 0, banner_reward = 5000;

    private int reward_type = 0;

    public string help_name = "";

    private bool coins_animation = false;

    private int coins_animation_start = 0;

    private int coins_animation_target;

    private float coins_timer = 0;

    private void Start()
    {
        if (restart_button) restart_button.onClick.AddListener(GameController.instance.restart_logic);
        if (home_button) home_button.onClick.AddListener(home_logic);
        if (next_button) next_button.onClick.AddListener(next);
    }

    private void Update()
    {
        if (coins_animation)
        {
            coins_timer += Time.unscaledDeltaTime;
            float coef = coins_timer / 3;
            if (coef >= 1) { coef = 1; coins_animation = false; }
            reward_coins_text.text = ""+Mathf.RoundToInt(coins_animation_start + coef * (coins_animation_target - coins_animation_start));
        }
    }

    public void set_stars(int count)
    {
        if (count == 1)
        {
            stars[0].color = stars_colors[0];
            stars[1].color = stars_colors[3];
            stars[2].color = stars_colors[3];
        }
        else if (count == 2)
        {
            stars[0].color = stars_colors[1];
            stars[1].color = stars_colors[1];
            stars[2].color = stars_colors[3];
        }
        else
        {
            stars[0].color = stars_colors[2];
            stars[1].color = stars_colors[2];
            stars[2].color = stars_colors[2];
        }

        int level_stars = PlayerPrefs.GetInt("level_" + GameLogic.instance.current_level + "_stars");
        if (count > level_stars)
            PlayerPrefs.SetInt("level_" + GameLogic.instance.current_level + "_stars", count);

        if (GameLogic.instance.current_level != 0)
        {
            int reward_number = PlayerPrefs.GetInt("reward_count_" + GameLogic.instance.current_level);
            PlayerPrefs.SetInt("reward_count_" + GameLogic.instance.current_level, reward_number + 1);
            reward_type = 0;
            if (reward_number == 0) { reward = 50000; reward_type = 2; banner_reward = 10000; }
            else if (reward_number == 1) { reward = 25000; reward_type = 1; }

            else if (GameLogic.instance.current_level < 5) reward = 750;
            else if (GameLogic.instance.current_level < 10) reward = 1500;
            else if (GameLogic.instance.current_level < 15) reward = 2500;
            else reward = 5000;

            if (PlayerPrefs.GetInt("money_x2_on") == 1)
            {
                reward *= 2;
                banner_reward *= 2;
            }

            Player.instance.add_money(reward);
            if (PlayerPrefs.GetInt("banners_activated") == 1) Player.instance.add_money(banner_reward);
            player_money_text.text = Player.instance.get_money();
        }

        StartCoroutine(show_star(count));
    }

    private IEnumerator show_star(int starts_count)
    {
     //   if (PlayerPrefs.GetInt("banners_activated") == 1) FirebaseManager.instance.show_banner(false);

        yield return new WaitForSecondsRealtime(1);
        for (int star_index = 0; star_index < starts_count; star_index++)
        {
            stars[star_index].gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(1);
        }
        if (reward_type == 2) animator.Play("chests_apear");
        else if (reward_type == 1) animator.Play("chests_apear2");
        else animator.Play("chests_apear3");




        yield return new WaitForSecondsRealtime(1.3f);
        PlayerAudio.instance.play(1);
        yield return new WaitForSecondsRealtime(.3f);
        PlayerAudio.instance.play(1);


        if (reward_type == 2)
        {
            yield return new WaitForSecondsRealtime(2);
            animator.Play("chest1_open");
        }
        else if (reward_type == 1)
        {
            yield return new WaitForSecondsRealtime(2);
            animator.Play("chest2_open");
        }
        yield return new WaitForSecondsRealtime(1);

        coins_animation_target = reward;
        coins_animation = true;
        PlayerAudio.instance.play(3);

        yield return new WaitForSecondsRealtime(4);

        if (PlayerPrefs.GetInt("banners_activated") == 1) // Если баннерная реклама была включена.
        {
            yield return new WaitForSecondsRealtime(2);
            if (reward_type == 2) animator.Play("chests_move2");
            else if (reward_type == 1) animator.Play("chests_move3");
            else animator.Play("chests_move4");

            yield return new WaitForSecondsRealtime(1);

            coins_timer = 0;
            coins_animation_target = reward + banner_reward;
            coins_animation = true;
            coins_animation_start = reward;
            PlayerAudio.instance.play(3);
            yield return new WaitForSecondsRealtime(4);
            //if (PlayerPrefs.GetInt("banners_activated") == 1) AppodealManager.instance.show_banner(true);

        }

        bool rateUsShown = AppReviewManager.Instance.CheckAfterLevelCompletion();

        if (help_name == "")
        {
            if (Checkpoints.current_save != null) Checkpoints.current_save = null;
            if (WavesCreator.instance) WavesCreator.waves = null;

            if (rateUsShown)
            {
                AppReviewManager.Instance.spawnedRateUsPrefab.GetComponent<RateUsPanel>().AddSceneSwitchToButtons("episode_1");
            }
            else
            {
                yield return new WaitForSecondsRealtime(4);

                LevelLoader.instance.loadScene("episode_1");
            }



        }
        else
        {
            if (Checkpoints.current_save != null) Checkpoints.current_save = null;
            if (WavesCreator.instance) WavesCreator.waves = null;
            PlayerPrefs.SetInt(help_name + "_done", 1);

            if (rateUsShown)
            {
                AppReviewManager.Instance.spawnedRateUsPrefab.GetComponent<RateUsPanel>().AddSceneSwitchToButtons(help_name);
            }
            else
            {
                yield return new WaitForSecondsRealtime(1);

                LevelLoader.instance.loadScene(help_name);
            }
        }
    }

    private void next()
    {
        int next_level = GameLogic.instance.current_level + 1;

        if (Checkpoints.current_save != null) Checkpoints.current_save = null;

        if (next_level != 21 && next_level != 36 && next_level != 56)
        {

            LevelLoader.instance.loadScene("level_" + next_level);

           /* if (PlayerPrefs.GetString("RateUsShown") == "shown")
            {
                AdsManager.EResultCode code = AdsManager.ShowInter("inter_next_level");
            }*/
        }
        else
            LevelLoader.instance.loadScene("episode_1");
    }

    private void home_logic()
    {
        if (Checkpoints.current_save != null) Checkpoints.current_save = null;
        if (WavesCreator.instance) WavesCreator.waves = null;
        LevelLoader.instance.loadScene("episode_1");
    }
}

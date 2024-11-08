public interface ITank
{
    int GetLives();
    void SetLives(int lives);

    void Hit();

    void Destroy();

    void SetShooting(bool shooting);
}

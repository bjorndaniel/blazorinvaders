using System.Drawing;
using BlazorInvaders.GameObjects;
using Xunit;

namespace BlazorInvaders.Tests;

public class SpriteTests
{
    [Fact]
    public void Size_ReturnsCorrectDimensions()
    {
        var sprite = new Sprite(0, 0, 20, 12);

        Assert.Equal(new Size(20, 12), sprite.Size);
    }

    [Fact]
    public void RenderSize_IsDoubleTheSourceSize()
    {
        var sprite = new Sprite(0, 0, 20, 12);

        Assert.Equal(new Size(40, 24), sprite.RenderSize);
    }

    [Fact]
    public void TopLeft_StoresCorrectCoordinates()
    {
        var sprite = new Sprite(5, 10, 25, 30);

        Assert.Equal(new Point(5, 10), sprite.TopLeft);
    }

    [Fact]
    public void BottomRight_StoresCorrectCoordinates()
    {
        var sprite = new Sprite(5, 10, 25, 30);

        Assert.Equal(new Point(25, 30), sprite.BottomRight);
    }
}

public class ShotTests
{
    [Fact]
    public void Move_DecreasesYBy20()
    {
        var shot = new Shot(new Point(100, 200));
        shot.Move();

        Assert.Equal(new Point(100, 180), shot.CurrentPosition);
    }

    [Fact]
    public void Move_MultipleTimes_AccumulatesMovement()
    {
        var shot = new Shot(new Point(50, 300));
        shot.Move();
        shot.Move();
        shot.Move();

        Assert.Equal(new Point(50, 240), shot.CurrentPosition);
    }

    [Fact]
    public void Remove_DefaultIsFalse()
    {
        var shot = new Shot(new Point(100, 200));

        Assert.False(shot.Remove);
    }

    [Fact]
    public void Sprite_HasCorrectCoordinates()
    {
        var shot = new Shot(new Point(0, 0));

        Assert.Equal(new Point(20, 60), shot.Sprite.TopLeft);
        Assert.Equal(new Point(22, 66), shot.Sprite.BottomRight);
    }
}

public class BombTests
{
    [Fact]
    public void Move_IncreasesYBy10()
    {
        var bomb = new Bomb(new Point(100, 200));
        bomb.Move();

        Assert.Equal(new Point(100, 210), bomb.CurrentPosition);
    }

    [Fact]
    public void Move_MultipleTimes_AccumulatesMovement()
    {
        var bomb = new Bomb(new Point(50, 100));
        bomb.Move();
        bomb.Move();

        Assert.Equal(new Point(50, 120), bomb.CurrentPosition);
    }

    [Fact]
    public void Remove_DefaultIsFalse()
    {
        var bomb = new Bomb(new Point(100, 200));

        Assert.False(bomb.Remove);
    }
}

public class PlayerTests
{
    [Fact]
    public void Constructor_PlacesPlayerAtHorizontalCenter()
    {
        var player = new Player(800, 500);

        Assert.Equal(400, player.CurrentPosition.X);
    }

    [Fact]
    public void Constructor_PlacesPlayerNearBottom()
    {
        var player = new Player(800, 500);

        Assert.Equal(450, player.CurrentPosition.Y);
    }

    [Fact]
    public void Move_Left_DecreasesX()
    {
        var player = new Player(800, 500);
        var startX = player.CurrentPosition.X;
        player.Move(20, Direction.Left);

        Assert.Equal(startX - 20, player.CurrentPosition.X);
    }

    [Fact]
    public void Move_Right_IncreasesX()
    {
        var player = new Player(800, 500);
        var startX = player.CurrentPosition.X;
        player.Move(20, Direction.Right);

        Assert.Equal(startX + 20, player.CurrentPosition.X);
    }

    [Fact]
    public void Move_Left_DoesNotExceedLeftBoundary()
    {
        var player = new Player(800, 500);
        for (int i = 0; i < 100; i++)
            player.Move(20, Direction.Left);

        Assert.True(player.CurrentPosition.X > 0);
    }

    [Fact]
    public void Move_Right_DoesNotExceedRightBoundary()
    {
        var player = new Player(800, 500);
        for (int i = 0; i < 100; i++)
            player.Move(20, Direction.Right);

        Assert.True(player.CurrentPosition.X < 800);
    }

    [Fact]
    public void Move_DoesNotChangeY()
    {
        var player = new Player(800, 500);
        var startY = player.CurrentPosition.Y;
        player.Move(20, Direction.Left);
        player.Move(20, Direction.Right);

        Assert.Equal(startY, player.CurrentPosition.Y);
    }

    [Fact]
    public void Collision_WithBomb_DirectHit_ReturnsTrue()
    {
        var player = new Player(800, 500);
        var bomb = new Bomb(player.CurrentPosition);

        Assert.True(player.Collision(bomb));
    }

    [Fact]
    public void Collision_WithBomb_FarAway_ReturnsFalse()
    {
        var player = new Player(800, 500);
        var bomb = new Bomb(new Point(0, 0));

        Assert.False(player.Collision(bomb));
    }

    [Fact]
    public void Collision_WithAlien_DirectHit_ReturnsTrue()
    {
        var player = new Player(800, 500);
        var alien = new Alien(AlienType.Crab, player.CurrentPosition, 0, 0);

        Assert.True(player.Collision(alien));
    }

    [Fact]
    public void Collision_WithAlien_FarAway_ReturnsFalse()
    {
        var player = new Player(800, 500);
        var alien = new Alien(AlienType.Crab, new Point(0, 0), 0, 0);

        Assert.False(player.Collision(alien));
    }

    [Fact]
    public void HasBeenHit_SetToTrue_SpriteChangesToExplosion()
    {
        var player = new Player(800, 500);
        player.HasBeenHit = true;

        // explosion sprite starts at (0,47)
        Assert.Equal(new Point(0, 47), player.Sprite.TopLeft);
    }

    [Fact]
    public void HasBeenHit_False_SpriteIsShip()
    {
        var player = new Player(800, 500);

        // ship sprite starts at (20,47)
        Assert.Equal(new Point(20, 47), player.Sprite.TopLeft);
    }
}

public class AlienTests
{
    [Theory]
    [InlineData(AlienType.Squid)]
    [InlineData(AlienType.Crab)]
    [InlineData(AlienType.Octopus)]
    public void Constructor_AllTypes_SetsStartPosition(AlienType type)
    {
        var start = new Point(100, 200);
        var alien = new Alien(type, start, 0, 0);

        Assert.Equal(start, alien.CurrentPosition);
    }

    [Fact]
    public void Constructor_SetsColumnAndRow()
    {
        var alien = new Alien(AlienType.Crab, new Point(100, 100), 3, 2);

        Assert.Equal(3, alien.Column);
        Assert.Equal(2, alien.Row);
    }

    [Fact]
    public void MoveDown_IncreasesYBy20()
    {
        var alien = new Alien(AlienType.Squid, new Point(100, 100), 0, 0);
        alien.MoveDown();

        Assert.Equal(120, alien.CurrentPosition.Y);
        Assert.Equal(100, alien.CurrentPosition.X);
    }

    [Fact]
    public void MoveHorizontal_Right_IncreasesXBy10()
    {
        var alien = new Alien(AlienType.Crab, new Point(100, 100), 0, 0);
        alien.MoveHorizontal(Direction.Right);

        Assert.Equal(110, alien.CurrentPosition.X);
    }

    [Fact]
    public void MoveHorizontal_Left_DecreasesXBy10()
    {
        var alien = new Alien(AlienType.Crab, new Point(100, 100), 0, 0);
        alien.MoveHorizontal(Direction.Left);

        Assert.Equal(90, alien.CurrentPosition.X);
    }

    [Fact]
    public void MoveHorizontal_DoesNotChangeY()
    {
        var alien = new Alien(AlienType.Octopus, new Point(100, 200), 0, 0);
        alien.MoveHorizontal(Direction.Right);

        Assert.Equal(200, alien.CurrentPosition.Y);
    }

    [Fact]
    public void Collision_WithShot_DirectHit_ReturnsTrue()
    {
        var alien = new Alien(AlienType.Crab, new Point(100, 100), 0, 0);
        var shot = new Shot(alien.CurrentPosition);

        Assert.True(alien.Collision(shot));
    }

    [Fact]
    public void Collision_WithShot_FarAway_ReturnsFalse()
    {
        var alien = new Alien(AlienType.Crab, new Point(100, 100), 0, 0);
        var shot = new Shot(new Point(500, 500));

        Assert.False(alien.Collision(shot));
    }

    [Fact]
    public void HasBeenHit_SetToTrue_SpriteChangesToExplosion()
    {
        var alien = new Alien(AlienType.Squid, new Point(100, 100), 0, 0);
        alien.HasBeenHit = true;

        // explosion sprite starts at (0,59)
        Assert.Equal(new Point(0, 59), alien.Sprite.TopLeft);
    }

    [Fact]
    public void Destroyed_DefaultIsFalse()
    {
        var alien = new Alien(AlienType.Squid, new Point(100, 100), 0, 0);

        Assert.False(alien.Destroyed);
    }

    [Fact]
    public void Crab_HasCorrectSprite()
    {
        var alien = new Alien(AlienType.Crab, new Point(100, 100), 0, 0);

        Assert.Equal(new Point(0, 0), alien.Sprite.TopLeft);
    }

    [Fact]
    public void Octopus_HasCorrectSprite()
    {
        var alien = new Alien(AlienType.Octopus, new Point(100, 100), 0, 0);

        Assert.Equal(new Point(20, 14), alien.Sprite.TopLeft);
    }

    [Fact]
    public void Squid_HasCorrectSprite()
    {
        var alien = new Alien(AlienType.Squid, new Point(100, 100), 0, 0);

        Assert.Equal(new Point(0, 27), alien.Sprite.TopLeft);
    }

    [Fact]
    public void MoveHorizontal_TogglesSpriteBetweenStillAndMoving()
    {
        var alien = new Alien(AlienType.Crab, new Point(100, 100), 0, 0);
        var firstSprite = alien.Sprite.TopLeft;

        alien.MoveHorizontal(Direction.Right);
        var movingSprite = alien.Sprite.TopLeft;

        alien.MoveHorizontal(Direction.Right);
        var backToStill = alien.Sprite.TopLeft;

        Assert.NotEqual(firstSprite, movingSprite);
        Assert.Equal(firstSprite, backToStill);
    }
}

public class MotherShipTests
{
    [Fact]
    public void Constructor_SetsStartPosition()
    {
        var start = new Point(800, 80);
        var ship = new MotherShip(start);

        Assert.Equal(start, ship.CurrentPosition);
    }

    [Fact]
    public void Move_DecreasesXBy7()
    {
        var ship = new MotherShip(new Point(800, 80));
        ship.Move();

        Assert.Equal(793, ship.CurrentPosition.X);
        Assert.Equal(80, ship.CurrentPosition.Y);
    }

    [Fact]
    public void Move_DoesNotChangeY()
    {
        var ship = new MotherShip(new Point(800, 80));
        ship.Move();
        ship.Move();

        Assert.Equal(80, ship.CurrentPosition.Y);
    }

    [Fact]
    public void Collision_WithShot_DirectHit_ReturnsTrue()
    {
        var ship = new MotherShip(new Point(100, 80));
        var shot = new Shot(ship.CurrentPosition);

        Assert.True(ship.Collision(shot));
    }

    [Fact]
    public void Collision_WithShot_FarAway_ReturnsFalse()
    {
        var ship = new MotherShip(new Point(100, 80));
        var shot = new Shot(new Point(500, 400));

        Assert.False(ship.Collision(shot));
    }

    [Fact]
    public void HasBeenHit_SetToTrue_SpriteChangesToExplosion()
    {
        var ship = new MotherShip(new Point(100, 80));
        ship.HasBeenHit = true;

        // explosion sprite starts at (0,59)
        Assert.Equal(new Point(0, 59), ship.Sprite.TopLeft);
    }

    [Fact]
    public void HasBeenHit_False_SpriteIsShip()
    {
        var ship = new MotherShip(new Point(100, 80));

        // ship sprite starts at (12,73)
        Assert.Equal(new Point(12, 73), ship.Sprite.TopLeft);
    }

    [Fact]
    public void Destroyed_DefaultIsFalse()
    {
        var ship = new MotherShip(new Point(100, 80));

        Assert.False(ship.Destroyed);
    }
}

public class GameTimeTests
{
    [Fact]
    public void TotalTime_Set_UpdatesElapsedTime()
    {
        var gameTime = new GameTime();
        gameTime.TotalTime = 1000f;
        gameTime.TotalTime = 1016f;

        Assert.Equal(16f, gameTime.ElapsedTime);
    }

    [Fact]
    public void TotalTime_FirstSet_ElapsedEqualsValue()
    {
        var gameTime = new GameTime();
        gameTime.TotalTime = 500f;

        Assert.Equal(500f, gameTime.ElapsedTime);
    }
}

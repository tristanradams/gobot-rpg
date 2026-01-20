using Godot;

namespace RpgCSharp.scripts.autoload;

public partial class EventBus : Node
{
    // Game state signals
    [Signal] public delegate void GameStateChangedEventHandler(int newState);

    // Player signals
    [Signal] public delegate void PlayerHealthChangedEventHandler(int newHealth, int maxHealth);
    [Signal] public delegate void PlayerDiedEventHandler();
    [Signal] public delegate void PlayerRespawnedEventHandler();

    // Combat signals
    [Signal] public delegate void DamageDealtEventHandler(Node target, int amount, Node source);
    [Signal] public delegate void EnemyDefeatedEventHandler(Node enemy);

    // Inventory signals
    [Signal] public delegate void ItemPickedUpEventHandler(string itemId, int quantity);
    [Signal] public delegate void ItemUsedEventHandler(string itemId);
    [Signal] public delegate void InventoryChangedEventHandler();

    // Dialogue signals
    [Signal] public delegate void DialogueStartedEventHandler(string npcId);
    [Signal] public delegate void DialogueEndedEventHandler();
    [Signal] public delegate void DialogueChoiceMadeEventHandler(int choiceIndex);

    // Quest signals
    [Signal] public delegate void QuestStartedEventHandler(string questId);
    [Signal] public delegate void QuestUpdatedEventHandler(string questId);
    [Signal] public delegate void QuestCompletedEventHandler(string questId);

    // UI signals
    [Signal] public delegate void NotificationRequestedEventHandler(string message);
}
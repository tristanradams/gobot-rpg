extends Node

# Game state signals
signal game_state_changed(new_state: int)

# Player signals
signal player_health_changed(new_health: int, max_health: int)
signal player_died
signal player_respawned

# Combat signals
signal damage_dealt(target: Node, amount: int, source: Node)
signal enemy_defeated(enemy: Node)

# Inventory signals
signal item_picked_up(item_id: String, quantity: int)
signal item_used(item_id: String)
signal inventory_changed

# Dialogue signals
signal dialogue_started(npc_id: String)
signal dialogue_ended
signal dialogue_choice_made(choice_index: int)

# Quest signals
signal quest_started(quest_id: String)
signal quest_updated(quest_id: String)
signal quest_completed(quest_id: String)

# UI signals
signal notification_requested(message: String)

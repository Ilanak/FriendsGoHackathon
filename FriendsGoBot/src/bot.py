import telegram
from telegram.ext import Updater, CommandHandler, MessageHandler, Filters
import logging
import requests

# Enable logging
logging.basicConfig(
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    level=logging.INFO)

logger = logging.getLogger(__name__)


def error(bot, update, error):
    logger.warn('Update "%s" caused error "%s"' % (update, error))

def handleCheckinCommand(bot, update):
    button = telegram.KeyboardButton("Share my location", request_location=True)
    customKeyboard = [[button]]
    reply_markup = telegram.ReplyKeyboardMarkup(keyboard=customKeyboard, resize_keyboard=True, one_time_keyboard=True)
    bot.send_message(update.message.from_user.id, "Share your location", reply_markup=reply_markup)

def handleJoinCommand(bot, update):
    join = {"summary": "join", "chat_id": update.message.chat_id,"user_id": update.message.from_user.id}
    resp = requests.post('https://todolist.example.com/tasks/', json=join)
    if resp.status_code == 201:
        bot.send_message(update.message.chat_id, "You joined the game!")

def handleChallengeCommand(bot, update):
    challenge = {"summary": "challenge", "chat_id": update.message.chat_id}
    resp = requests.post('https://todolist.example.com/tasks/', json=challenge)
    if resp.status_code == 200:
        bot.send_message(update.message.chat_id, "YourChallengeIs:")
        for location_item in resp.json():
            bot.send_message(update.message.chat_id, "get to:%s,%s", location_item['id'], location_item['count'])

def handleStatCommand(bot, update):
    bot.send_message(update.message.chat_id, "Status: ")

#task = {"summary": "Take out trash", "description": "" }
#resp = requests.post('https://todolist.example.com/tasks/', json=task)

#change to py string str(update.message.chat_id)


def handleMyLocationCommand(bot, update):
    bot.send_message(update.message.chat_id, "Thanks")

def main():
    updater = Updater(token="269182723:AAG26YGMIwHaSi6oGQUdM2kABsdeMVNKSdA")
    dp = updater.dispatcher
    dp.add_handler(CommandHandler("join", handleJoinCommand))
    dp.add_handler(CommandHandler("challenge", handleChallengeCommand))
    dp.add_handler(CommandHandler("stat", handleStatCommand))
    dp.add_handler(CommandHandler("checkin", handleCheckinCommand))

    dp.add_handler(MessageHandler([Filters.location], handleMyLocationCommand))
    dp.add_error_handler(error)

    updater.start_polling()

    # Run the bot until the you presses Ctrl-C or the process receives SIGINT,
    # SIGTERM or SIGABRT. This should be used most of the time, since
    # start_polling() is non-blocking and will stop the bot gracefully.
    updater.idle()

main()




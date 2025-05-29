import javax.swing.*;
import java.awt.*;
import java.awt.event.ActionEvent;
import java.util.Random;

public class MainFlow {

    private static final String HOME = "home";
    private static final String OPEN_ORDERS = "openOrders";
    private static final String TRANSACTION_DETAIL = "transactionDetail";

    public static void main(String[] args) {
        SwingUtilities.invokeLater(MainFlow::createAndShowGui);
    }

    private static void createAndShowGui() {
        JFrame frame = new JFrame("MainFlow");
        frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        frame.setSize(400, 200);

        CardLayout cardLayout = new CardLayout();
        JPanel cards = new JPanel(cardLayout);

        // Home screen with two identical buttons
        JPanel homePanel = new JPanel();
        JButton openOrdersBtn1 = new JButton("Open Orders By Store");
        openOrdersBtn1.addActionListener(e ->
            delayShow(OPEN_ORDERS, cardLayout, cards)
        );
        JButton openOrdersBtn2 = new JButton("Open Orders By Store");
        openOrdersBtn2.addActionListener(e ->
            delayShow(OPEN_ORDERS, cardLayout, cards)
        );
        homePanel.add(openOrdersBtn1);
        homePanel.add(openOrdersBtn2);

        // Open Orders By Store screen
        JPanel openOrdersPanel = new JPanel(new FlowLayout());
        JButton transactionDetailBtn = new JButton("Transaction Detail");
        transactionDetailBtn.addActionListener(e ->
            delayShow(TRANSACTION_DETAIL, cardLayout, cards)
        );
        JButton closeBtn = new JButton("CLOSE");
        closeBtn.addActionListener(e ->
            delayShow(HOME, cardLayout, cards)
        );
        openOrdersPanel.add(transactionDetailBtn);
        openOrdersPanel.add(closeBtn);

        // Transaction Details screen
        JPanel transactionPanel = new JPanel();
        JButton backBtn = new JButton("Back");
        backBtn.addActionListener(e ->
            delayShow(OPEN_ORDERS, cardLayout, cards)
        );
        transactionPanel.add(backBtn);

        // add cards
        cards.add(homePanel, HOME);
        cards.add(openOrdersPanel, OPEN_ORDERS);
        cards.add(transactionPanel, TRANSACTION_DETAIL);

        frame.getContentPane().add(cards);
        frame.setLocationRelativeTo(null);
        frame.setVisible(true);
    }

    private static void delayShow(String name, CardLayout layout, JPanel cards) {
        int delayMs = 2000 + new Random().nextInt(3000); // 2000–4999ms
        new Timer(delayMs, (ActionEvent e) -> {
            layout.show(cards, name);
            ((Timer)e.getSource()).stop();
        }).start();
    }
}
